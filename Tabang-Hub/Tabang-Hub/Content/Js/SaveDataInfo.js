document.addEventListener('DOMContentLoaded', function () {
    // Add event listener to the Submit button in the Done section
    document.querySelector('.main:last-child .next').addEventListener('click', function () {
        // Hide the popup
        document.getElementById('modal-overlay').style.display = 'none';
        // Set a flag in local storage to indicate that the popup has been shown
        localStorage.setItem('popupShown', true);
    });

    // Call onNewUserLogin when a new user logs in
    function onNewUserLogin() {
        resetProgressBar();
        document.getElementById('modal-overlay').style.display = 'block';
    }

    // Example usage: Call this function when a new user logs in
    onNewUserLogin();
});

function SaveInformations() {
    console.log("Save button click");
    var fname = document.getElementById('fname').value;
    var lname = document.getElementById('lname').value;
    var bday = document.getElementById('datepicker').value;
    var phoneNum = document.getElementById('phoneNumber').value;
    var street = document.getElementById('street').value;
    var city = document.getElementById('city').value;
    var province = document.getElementById('province').value;
    var zipCode = document.getElementById('zipCode').value;
    var gender = getSelectedGender();

    var informationData = {
        fname: fname,
        lname: lname,
        bday: bday,
        phoneNum: phoneNum,
        street: street,
        city: city,
        province: province,
        zipCode: zipCode,
        gender: gender
    };

    var volunteerSkill = logSelectedSkills();
    SaveDatas(informationData, volunteerSkill);
}

function getSelectedGender() {
    var gender = document.getElementsByName('gender');
    var selectedGender;

    for (var i = 0; i < gender.length; i++) {
        if (gender[i].checked) {
            selectedGender = gender[i].value;
            break;
        }
    }
    return selectedGender;
}

function SaveDatas(informationData, volunteerSkill) {
    console.log("Save informations Function: ", informationData);
    console.log("Log Selected Skills: ", volunteerSkill);

    console.log("Save datas");
    $.ajax({
        type: 'POST',
        url: '/Volunteer/SaveInformation',
        contentType: 'application/json',
        data: JSON.stringify({
            model: informationData,
            volunteerSkill: volunteerSkill
        }),
        success: function (response) {
            console.log("Response is: ", response);
            location.reload();
        },
        error: function (xhr, status, error) {
            console.log("Error response is: ", error);
        }
    });
}
