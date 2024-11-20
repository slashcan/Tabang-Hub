from flask import Flask, request, jsonify
import pandas as pd
import joblib
import numpy as np
from sklearn.metrics.pairwise import cosine_similarity

app = Flask(__name__)

# Load the pre-trained Random Forest model
try:
    model = joblib.load('volunteer_skill_matching_model.pkl')
    print("Pretrained model loaded.")
except:
    print("No pretrained model found. Please train the model first.")

# Helper function to calculate Content-Based Filtering (CBF) similarity using cosine similarity
def calculate_cbf_similarity(user_skills, event_skills):
    # Create a set of all unique skills from both user and event skills
    all_skills = list(set(user_skills).union(set(event_skills)))
    
    # Convert user and event skills to one-hot vectors
    user_vector = [1 if skill in user_skills else 0 for skill in all_skills]
    event_vector = [1 if skill in event_skills else 0 for skill in all_skills]
    
    # Calculate Cosine Similarity between the vectors
    user_vector = np.array(user_vector).reshape(1, -1)
    event_vector = np.array(event_vector).reshape(1, -1)
    
    similarity = cosine_similarity(user_vector, event_vector)[0][0]
    
    return similarity

# Route 1: Skill Filtering and Event Matching
@app.route('/predict', methods=['POST'])
def predict_for_user():
    data = request.get_json()

    # Convert received data to Pandas DataFrames
    user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
    event_data = pd.DataFrame(data['event_data'], columns=['eventId', 'eventDescription'])
    event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])

    filtered_events = []
    volunteer_skills = set(user_skills_data['skillId'].tolist())

    for _, event_row in event_data.iterrows():
        event_id = event_row['eventId']
        required_skills = set(event_skills_data[event_skills_data['eventId'] == event_id]['skillId'].tolist())

        # Ensure all required event skills are present in the volunteer's skills
        if required_skills.issubset(volunteer_skills):
            # Calculate CBF similarity
            similarity_score = calculate_cbf_similarity(volunteer_skills, required_skills)
            if similarity_score >= 0.5:  # Threshold can be adjusted
                filtered_events.append({
                    'eventId': event_id,
                    'eventDescription': event_row['eventDescription'],
                    'requiredSkills': list(required_skills),
                    'similarityScore': similarity_score
                })

    return jsonify(filtered_events)

# Route 2: Recruitment Filtering based on Skill Match and Volunteer Availability
@app.route('/recruit', methods=['POST'])
def recruit_for_event():
    data = request.get_json()

    # Convert received data to Pandas DataFrames
    user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId', 'rating'])
    event_data = pd.DataFrame(data['event_data'], columns=['eventId', 'eventDescription'])
    event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])
    volunteer_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'availability'])

    filtered_volunteers = []

    # Step 1: Filter volunteers based on skill match for each event
    event_id = event_data['eventId'].values[0]
    required_skills = set(event_skills_data[event_skills_data['eventId'] == event_id]['skillId'].tolist())

    for user_id, group in user_skills_data.groupby('userId'):
        volunteer_skills = set(group['skillId'].tolist())
        matched_ratings = group[group['skillId'].isin(required_skills)]['rating'].tolist()

        # Ensure all required skills are in the volunteer's skills
        if required_skills.issubset(volunteer_skills):
            # Calculate CBF similarity
            similarity_score = calculate_cbf_similarity(volunteer_skills, required_skills)
            availability = volunteer_info_data[volunteer_info_data['userId'] == user_id]['availability'].values[0]
            avg_rating = np.mean(matched_ratings) if matched_ratings else 0

            filtered_volunteers.append({
                'userId': user_id,
                'matchedSkills': list(volunteer_skills),
                'rating': avg_rating,
                'similarityScore': similarity_score,
                'availability': availability
            })

    # Step 2: Sort volunteers by rating and similarity score
    ranked_volunteers_by_rating = sorted(filtered_volunteers, key=lambda x: (-x['rating'], -x['similarityScore']))

    # Step 3: Sort volunteers by availability (prioritizing Full Time) and then by rating
    availability_order = {'Full Time': 0, 'Part Time': 1}  # Order for sorting availability
    ranked_volunteers_by_availability = sorted(filtered_volunteers, key=lambda x: (availability_order.get(x['availability'], 2), -x['rating'], -x['similarityScore']))

    return jsonify({
        'sortedByRating': ranked_volunteers_by_rating,
        'sortedByAvailability': ranked_volunteers_by_availability
    })

if __name__ == '__main__':
    app.run(debug=True, port=5000)
