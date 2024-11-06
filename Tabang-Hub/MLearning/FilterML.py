from flask import Flask, request, jsonify
import pandas as pd
from sklearn.metrics.pairwise import cosine_similarity
from sklearn.linear_model import LogisticRegression
import joblib
import numpy as np

app = Flask(__name__)

# Load the pre-trained Logistic Regression and CBF models
try:
    logistic_model = joblib.load('volunteer_logistic_model.pkl')
    cbf_model = joblib.load('volunteer_cbf_model.pkl')
    print("Pretrained Logistic and CBF models loaded.")
except:
    logistic_model = LogisticRegression()
    cbf_model = LogisticRegression()
    print("No pretrained models found. Please train the models first.")

# Helper function to calculate cosine similarity
def calculate_similarity(user_skills, event_skills):
    user_vector = np.array([1 if skill in user_skills else 0 for skill in event_skills])
    event_vector = np.ones(len(event_skills))

    # Prevent division by zero
    user_norm = np.linalg.norm(user_vector)
    event_norm = np.linalg.norm(event_vector)

    if user_norm == 0 or event_norm == 0:
        return 0.0  # Return 0 if the vectors are zero

    return np.dot(user_vector, event_vector) / (user_norm * event_norm)

# Route 1: Skill Filtering and Attendance Prediction with Full Skill Match Requirement
@app.route('/predict', methods=['POST'])
def predict_for_user():
    data = request.get_json()

    # Convert received data to Pandas DataFrames
    user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
    event_data = pd.DataFrame(data['event_data'], columns=['eventId', 'eventDescription'])
    event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])
    volunteer_history_data = pd.DataFrame(data['volunteer_history'], columns=['eventId', 'attended']) if data.get('volunteer_history') else pd.DataFrame()

    # Step 1: Apply Logistic Regression based on volunteer history (if available)
    if not volunteer_history_data.empty:
        X = volunteer_history_data[['eventId', 'attended']]
        y = volunteer_history_data['attended']

        if len(y.unique()) > 1:
            logistic_model.fit(X, y)
            event_data['predictedAttendance'] = logistic_model.predict(event_data[['eventId']])
            predicted_events = event_data[event_data['predictedAttendance'] == 1]
        else:
            predicted_events = event_data
    else:
        predicted_events = event_data

    # Step 2: Filter events such that all required skills are matched by the volunteer's skills
    filtered_events = []
    volunteer_skills = set(user_skills_data['skillId'].tolist())

    for _, event_row in predicted_events.iterrows():
        event_id = event_row['eventId']
        required_skills = set(event_skills_data[event_skills_data['eventId'] == event_id]['skillId'].tolist())

        # Ensure all required event skills are present in the volunteer's skills
        if required_skills.issubset(volunteer_skills):
            similarity_score = calculate_similarity(volunteer_skills, required_skills)
            if similarity_score >= 0.5:  # This threshold can be adjusted as needed
                filtered_events.append({
                    'eventId': event_id,
                    'eventDescription': event_row['eventDescription'],
                    'requiredSkills': list(required_skills),
                    'similarityScore': similarity_score
                })

    return jsonify(filtered_events)

# Route 2: Recruitment Filtering with Full Skill Match and Rating Sorting
@app.route('/recruit', methods=['POST'])
def recruit_for_event():
    data = request.get_json()

    # Convert received data to Pandas DataFrames
    user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId', 'rating'])
    event_data = pd.DataFrame(data['event_data'], columns=['eventId', 'eventDescription'])
    event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])
    volunteer_history_data = pd.DataFrame(data['volunteer_history'], columns=['eventId', 'attended']) if data.get('volunteer_history') else pd.DataFrame()

    # Step 1: Apply Logistic Regression for attendance prediction
    if not volunteer_history_data.empty:
        X = volunteer_history_data[['eventId']]
        y = volunteer_history_data['attended']

        if len(y.unique()) > 1:
            logistic_model.fit(X, y)
            event_data['predictedAttendance'] = logistic_model.predict(event_data[['eventId']])
        else:
            event_data['predictedAttendance'] = 1  # Assume all events will be attended

        predicted_events = event_data[event_data['predictedAttendance'] == 1]
    else:
        predicted_events = event_data

    # Step 2: Filter volunteers based on full required skill match for each event
    event_id = predicted_events['eventId'].values[0]
    required_skills = set(event_skills_data[event_skills_data['eventId'] == event_id]['skillId'].tolist())

    filtered_volunteers = []
    for user_id, group in user_skills_data.groupby('userId'):
        volunteer_skills = set(group['skillId'].tolist())
        volunteer_ratings = group['rating'].mean()

        # Ensure all required skills are in the volunteer's skills
        if required_skills.issubset(volunteer_skills):
            similarity_score = calculate_similarity(volunteer_skills, required_skills)
            filtered_volunteers.append({
                'userId': user_id,
                'matchedSkills': list(volunteer_skills),
                'rating': volunteer_ratings,
                'similarityScore': similarity_score
            })

    # Step 3: Sort volunteers by rating and similarity score
    ranked_volunteers = sorted(filtered_volunteers, key=lambda x: (-x['rating'], -x['similarityScore']))

    return jsonify(ranked_volunteers)

if __name__ == '__main__':
    app.run(debug=True, port=5000)
