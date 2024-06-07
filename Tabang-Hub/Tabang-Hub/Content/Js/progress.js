var click_next = document.querySelector(".next");
var click_back = document.querySelector(".back_page");
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

click_next.addEventListener('click', function () {
    if (!validateform()) {
        return false;
    }
    formnumber++;
    updateform();
    progress_forward();
});

click_back.addEventListener('click', function () {
    formnumber--;
    updateform();
    progress_back();
});

click_submit.addEventListener('click', function () {
    if (!validateform()) {
        return false;
    }
    formnumber++;
    updateform();
});

// Call this function when a new user logs in
function onNewUserLogin() {
    resetProgressBar();
    // Display the modal for the new user
    document.getElementById('modal-overlay').style.display = 'block';
}

// Example usage: Call this function when a new user logs in
onNewUserLogin();
