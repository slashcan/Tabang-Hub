from flask import Flask, request, jsonify
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score
from sklearn.metrics.pairwise import cosine_similarity
import numpy as np

app = Flask(__name__)

@app.route('/predict', methods=['POST'])
def predict_for_user():
    data = request.get_json()

    # Convert received data to Pandas DataFrames
    user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
    event_data = pd.DataFrame(data['event_data'], columns=['eventId', 'eventDescription'])
    event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])
    volunteer_history_data = pd.DataFrame(data['volunteer_history'], columns=['eventId', 'attended']) if data.get('volunteer_history') else pd.DataFrame()

    # Step 1: Apply Logistic Regression based on attendance history (if available)
    if not volunteer_history_data.empty:
        X = volunteer_history_data[['eventId', 'attended']]
        y = volunteer_history_data['attended']

        if len(y.unique()) > 1:
            X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
            model = LogisticRegression()
            model.fit(X_train, y_train)

            event_data['predictedAttendance'] = model.predict(event_data[['eventId']])

            # Calculate model accuracy for debugging
            y_pred = model.predict(X_test)
            accuracy = accuracy_score(y_test, y_pred)
            print(f"Model Accuracy: {accuracy:.2f}")

            predicted_events = event_data[event_data['predictedAttendance'] == 1]
        else:
            predicted_events = event_data
    else:
        predicted_events = event_data

    # Step 2: Filter out events the user has already attended
    if not volunteer_history_data.empty:
        attended_events = volunteer_history_data['eventId'].unique()
        predicted_events = predicted_events[~predicted_events['eventId'].isin(attended_events)]

    # Step 3: Use algorithm for skill matching (cosine similarity)
    filtered_events = []
    for _, event_row in predicted_events.iterrows():
        event_id = event_row['eventId']
        required_skills = event_skills_data[event_skills_data['eventId'] == event_id]['skillId'].tolist()

        # Convert the skills into binary vectors for cosine similarity
        user_skill_vector = [1 if skill in user_skills_data['skillId'].tolist() else 0 for skill in required_skills]
        event_skill_vector = [1 for _ in required_skills]  # All required skills present in the event

        # Calculate cosine similarity between user skills and event skills
        similarity_score = cosine_similarity([user_skill_vector], [event_skill_vector])[0][0]

        # Set a similarity threshold to determine a match (e.g., 0.5 or higher)
        if similarity_score >= 0.5:
            filtered_events.append({
                'eventId': event_id,
                'eventDescription': event_row['eventDescription'],
                'requiredSkills': required_skills,
                'matchedSkills': user_skills_data[user_skills_data['skillId'].isin(required_skills)]['skillId'].tolist(),
                'similarityScore': similarity_score
            })

    # Return the filtered events as JSON response to ASP.NET
    return jsonify(filtered_events)

if __name__ == '__main__':
    app.run(debug=True)
