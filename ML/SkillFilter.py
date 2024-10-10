from flask import Flask, request, jsonify
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score

# Create the Flask app
app = Flask(__name__)

@app.route('/predict', methods=['POST'])
def predict_for_user():
    data = request.get_json()  # Get JSON data from ASP.NET

    # Convert received data to Pandas DataFrames
    user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])  # Ensure columns match
    event_data = pd.DataFrame(data['event_data'], columns=['eventId', 'eventDescription'])  # Event information
    event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])  # Required skills for events
    volunteer_history_data = pd.DataFrame(data['volunteer_history'], columns=['eventId', 'attended']) if data.get('volunteer_history') else pd.DataFrame()  # User's volunteer history

    # Print for debugging purposes
    print("User Skills Data Columns:", user_skills_data.columns)
    print(user_skills_data)

    # Step 1: Apply Machine Learning Prediction Based on History (if available)
    if not volunteer_history_data.empty:
        # Prepare features for training the model using volunteer history
        X = volunteer_history_data[['eventId', 'attended']]
        y = volunteer_history_data['attended']

        if len(y.unique()) > 1:  # Only train if we have both classes (0 and 1)
            X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
            model = LogisticRegression()
            model.fit(X_train, y_train)

            # Predict attendance likelihood for all events based on the history
            event_data['predictedAttendance'] = model.predict(event_data[['eventId']])

            # Calculate and print model accuracy for debugging
            y_pred = model.predict(X_test)
            accuracy = accuracy_score(y_test, y_pred)
            print(f"Model Accuracy: {accuracy:.2f}")

            # Filter events with a high likelihood of attendance (predictedAttendance == 1)
            predicted_events = event_data[event_data['predictedAttendance'] == 1]
        else:
            # If there isn't enough class diversity, treat all events as possible
            predicted_events = event_data
    else:
        # If no volunteer history, treat all events as possible
        predicted_events = event_data

    # Step 2: Filter out events the user has already attended
    if not volunteer_history_data.empty:
        attended_events = volunteer_history_data['eventId'].unique()
        predicted_events = predicted_events[~predicted_events['eventId'].isin(attended_events)]

    # Step 3: Filter based on skill matching for new events
    filtered_events = []
    for _, event_row in predicted_events.iterrows():
        event_id = event_row['eventId']
        # Get the skills required for the current event
        required_skills = event_skills_data[event_skills_data['eventId'] == event_id]['skillId'].tolist()

        # Check if the user has matching skills
        skill_match = user_skills_data[user_skills_data['skillId'].isin(required_skills)]

        if not skill_match.empty:
            # Add the event to filtered events if there's a skill match
            filtered_events.append({
                'eventId': event_id,
                'eventDescription': event_row['eventDescription'],
                'requiredSkills': required_skills,
                'matchedSkills': skill_match['skillId'].tolist()
            })

    # Return the filtered events as JSON response to ASP.NET
    return jsonify(filtered_events)

if __name__ == '__main__':
    app.run(debug=True)
