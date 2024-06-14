function redirectToDetails(eventId) {
    window.location.href = '@Url.Action("EventDetails", "Volunteer")' + '?evetId=' + eventId;
}