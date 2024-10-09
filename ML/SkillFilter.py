from flask import Flask, request, jsonify
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score

# Create the Flask app
app = Flask(__name__)

# Load the new CSV files
volunteer_skills = pd.read_csv(r"TabangHubCSV/VolunteerSkill.csv")
org_skill_requirements = pd.read_csv(r"TabangHubCSV/OrgSkillRequirement.csv")
org_events = pd.read_csv(r"TabangHubCSV/OrgEvent.csv")
volunteer_history = pd.read_csv(r"TabangHubCSV/VolunteerHistory.csv")

# Merge data based on the new CSV structure
merged_data = pd.merge(volunteer_history, volunteer_skills, on='userId')
merged_data = pd.merge(merged_data, org_skill_requirements, on='eventID')
merged_data = pd.merge(merged_data, org_events, on='eventID')

# Update: Only skill matching logic
merged_data['skill_match'] = merged_data['skillID_x'] == merged_data['skillID_y']

# Convert boolean skill match to integer (1 for match, 0 for no match)
merged_data['skill_match'] = merged_data['skill_match'].astype(int)

# The final feature set for training the model, only skill matching
X = merged_data[['skill_match']]
y = merged_data['attended']

# Ensure we have both classes (1 for attended and 0 for not attended)
print("Class distribution in y:")
print(y.value_counts())

# Train the model
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42, stratify=y)
model = LogisticRegression()
model.fit(X_train, y_train)

# Calculate accuracy
y_pred = model.predict(X_test)
accuracy = accuracy_score(y_test, y_pred)
print(f"Model Accuracy: {accuracy:.2f}")

# Define an API route to handle the request
@app.route('/predict', methods=['POST'])
def predict_for_user():
    # Extract user ID from the request data (JSON)
    data = request.get_json()
    user_id = data.get('userId')

    # Get the user's skills
    user_skills_data = volunteer_skills[volunteer_skills['userId'] == user_id]
    matched_skills_dict = {}

    # Process and predict for the user, based only on skill matching
    for _, event_row in org_events.iterrows():
        event_id = event_row['eventID']
        event_skills_data = org_skill_requirements[org_skill_requirements['eventID'] == event_id]
        required_skills = ', '.join(event_skills_data['skillID'].astype(str).unique())
        skill_match_found = False
        matched_skills = set()

        for _, user_skill_row in user_skills_data.iterrows():
            for _, event_skill_row in event_skills_data.iterrows():
                if user_skill_row['skillID'] == event_skill_row['skillID']:
                    matched_skills.add(str(user_skill_row['skillID']))
                    skill_match_found = True

        if skill_match_found:
            matched_skills_dict[event_id] = {
                'eventDescription': event_row['eventDescription'],
                'requiredSkills': required_skills,
                'matchedSkills': ', '.join(matched_skills)
            }

    # Format the matched skills data for output
    matched_skills_output = []
    for event_id, event_data in matched_skills_dict.items():
        matched_skills_output.append({
            'eventID': event_id,
            'eventDescription': event_data['eventDescription'],
            'requiredSkills': event_data['requiredSkills'],
            'matchedSkills': event_data['matchedSkills']
        })

    # Return the response as JSON
    return jsonify(matched_skills_output)

# Run the Flask app
if __name__ == '__main__':
    app.run(debug=True)
