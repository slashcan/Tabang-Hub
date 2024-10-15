from flask import Flask, request, jsonify
import pandas as pd
from sklearn.linear_model import LogisticRegression
from sklearn.metrics.pairwise import cosine_similarity
import joblib

app = Flask(__name__)

# Load or create a Logistic Regression model
try:
    model = joblib.load('volunteer_recruitment_model.pkl')
    print("Pretrained Logistic Regression model loaded.")
except:
    model = LogisticRegression()
    print("No pretrained model found. Will train on demand.")

@app.route('/recruit', methods=['POST'])
def recruit_for_event():
    data = request.get_json()

    # Convert received data to Pandas DataFrames
    user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId', 'rating'])
    event_data = pd.DataFrame(data['event_data'], columns=['eventId', 'eventDescription'])
    event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])
    volunteer_history_data = pd.DataFrame(data['volunteer_history'], columns=['eventId', 'attended']) if data.get('volunteer_history') else pd.DataFrame()

    # Step 1: Apply Logistic Regression for prediction (if history is available)
    if not volunteer_history_data.empty:
        X = volunteer_history_data[['eventId']]
        y = volunteer_history_data['attended']

        if len(y.unique()) > 1:
            model.fit(X, y)
            event_data['predictedAttendance'] = model.predict(event_data[['eventId']])
        else:
            event_data['predictedAttendance'] = 1  # Assume all events are likely to be attended

        # Filter based on attendance prediction
        predicted_events = event_data[event_data['predictedAttendance'] == 1]
    else:
        predicted_events = event_data  # No history, assume all events are valid

    # Step 2: Filter volunteers based on required skills
    event_id = predicted_events['eventId'].values[0]
    required_skills = event_skills_data[event_skills_data['eventId'] == event_id]['skillId'].tolist()

    # Volunteers that match required skills
    matching_volunteers = user_skills_data[user_skills_data['skillId'].isin(required_skills)]

    # Step 3: Use cosine similarity to rank volunteers based on matching skills and ratings
    filtered_volunteers = []
    for user_id, group in matching_volunteers.groupby('userId'):
        volunteer_skills = group['skillId'].tolist()
        volunteer_ratings = group['rating'].mean()  # Average rating

        # Calculate cosine similarity between volunteer skills and event required skills
        user_skill_vector = [1 if skill in volunteer_skills else 0 for skill in required_skills]
        event_skill_vector = [1 for _ in required_skills]  # All required skills present in the event
        similarity_score = cosine_similarity([user_skill_vector], [event_skill_vector])[0][0]

        filtered_volunteers.append({
            'userId': user_id,
            'matchedSkills': volunteer_skills,
            'rating': volunteer_ratings,
            'similarityScore': similarity_score
        })

    # Step 4: Sort volunteers by rating and similarity score
    ranked_volunteers = sorted(filtered_volunteers, key=lambda x: (-x['rating'], -x['similarityScore']))

    # Prepare the response data
    response = []
    for idx, volunteer in enumerate(ranked_volunteers):
        response.append({
            'rank': idx + 1,
            'userId': volunteer['userId'],
            'matchedSkills': volunteer['matchedSkills'],
            'rating': volunteer['rating'],
            'similarityScore': volunteer['similarityScore']
        })

    return jsonify(response)

if __name__ == '__main__':
    app.run(debug=True)
