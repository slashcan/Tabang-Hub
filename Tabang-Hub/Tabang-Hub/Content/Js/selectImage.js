// Add event listener to selectable images
document.querySelectorAll('.selectable').forEach(item => {
    item.addEventListener('click', event => {
        // Toggle the 'selected' class on click
        event.target.classList.toggle('selected');
        // Check if any skill is selected
        checkSkillsSelected();
    });
});

// Function to check if any skill is selected
function checkSkillsSelected() {
    // Get all selected skills
    const selectedSkills = document.querySelectorAll('.selectable.selected');
    // Enable or disable the submit button based on selection
    const submitButton = document.querySelector('.submit');
    if (selectedSkills.length > 0) {
        submitButton.disabled = false;
    } else {
        submitButton.disabled = true;
    }
}
