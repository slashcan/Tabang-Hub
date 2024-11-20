import pandas as pd
from sklearn.ensemble import RandomForestClassifier
import joblib

# Load the training data
train_data = pd.read_csv('train_data.csv')  # Ensure this CSV file matches the provided structure

# Features: userId, eventId, and skillId
X = train_data[['userId', 'eventId', 'skillId']]  # Features (volunteer skill data)
y = train_data['match']   # Target: match (1 for match, 0 for no match)

# Train the RandomForestClassifier model
model = RandomForestClassifier(n_estimators=100, random_state=42)
model.fit(X, y)

# Save the trained model to a file
joblib.dump(model, 'volunteer_skill_matching_model.pkl')

print("Random Forest model trained and saved as 'volunteer_skill_matching_model.pkl'")
