import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.linear_model import LogisticRegression
import joblib

# Load the dataset
data = pd.read_csv('training_data.csv')

# Features (eventId) and labels (attended)
X = data[['eventId']]
y = data['attended']

# Split the data into training and testing sets
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

# Initialize and train the logistic regression model
model = LogisticRegression()
model.fit(X_train, y_train)

# Save the trained model to a file
joblib.dump(model, 'volunteer_recruitment_model.pkl')
print("Model trained and saved as volunteer_recruitment_model.pkl")
