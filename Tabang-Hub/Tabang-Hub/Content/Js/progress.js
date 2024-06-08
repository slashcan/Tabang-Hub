var click_next = document.querySelectorAll(".next");
var click_back = document.querySelectorAll(".back_page");
var click_submit = document.querySelector(".submit");
var click_main = document.querySelectorAll(".main");
var progress = document.querySelectorAll(".progress_bar li");

let formnumber = 0;

function resetProgressBar() {
    formnumber = 0;
    progress.forEach(function (step) {
        step.classList.remove('active');
    });
    progress[0].classList.add('active');
    updateform();
}

function updateform() {
    click_main.forEach(function (form) {
        form.classList.remove('active');
    });
    click_main[formnumber].classList.add('active');
}

function progress_forward() {
    progress[formnumber].classList.add('active');
}

function progress_back() {
    var back_p = formnumber + 1;
    progress[back_p].classList.remove('active');
}

function validateform() {
    var validate = true;
    var inputs = document.querySelectorAll(".main.active input");
    inputs.forEach(function (inpt) {
        inpt.classList.remove('warning');
        if (inpt.hasAttribute("required")) {
            if (inpt.value.length == "0") {
                validate = false;
                inpt.classList.add('warning');
            }
        }
    });
    return validate;
}

// Function to check if any skill is selected
function checkSkillsSelected() {
    // Get all selected skills
    const selectedSkills = document.querySelectorAll('.selectable.selected');
    // Enable or disable the next button based on selection
    click_next.forEach(function (button) {
        if (selectedSkills.length > 0) {
            button.disabled = false;
            button.style.backgroundColor = '#fb4552';
            button.style.cursor = 'pointer';
        } else {
            button.disabled = true;
            button.style.backgroundColor = 'grey';
            button.style.cursor = 'not-allowed';
        }
    });
}

// Add event listeners to selectable images
document.querySelectorAll('.selectable').forEach(item => {
    item.addEventListener('click', event => {
        // Toggle the 'selected' class on click
        event.target.classList.toggle('selected');
        // Check if any skill is selected
        checkSkillsSelected();
    });
});

// Add event listeners for all 'next' buttons
click_next.forEach(function (button) {
    button.addEventListener('click', function () {
        if (!validateform()) {
            return false;
        }
        formnumber++;
        updateform();
        progress_forward();
    });
});

// Add event listeners for all 'back' buttons
click_back.forEach(function (button) {
    button.addEventListener('click', function () {
        formnumber--;
        updateform();
        progress_back();
    });
});

// Event listener for the submit button
click_submit.addEventListener('click', function () {
    if (!validateform()) {
        return false;
    }
    formnumber++;
    updateform();
    progress_forward();
});

// Call this function when a new user logs in
function onNewUserLogin() {
    resetProgressBar();
    // Display the modal for the new user
    document.getElementById('modal-overlay').style.display = 'block';
}

// Example usage: Call this function when a new user logs in
onNewUserLogin();
