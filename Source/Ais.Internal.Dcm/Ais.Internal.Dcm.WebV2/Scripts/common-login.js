function bindSignOut() {
    $("#signout").click(function () {
        $(document).ajaxStop($.unblockUI);

        $.blockUI({ message: '<img src="/images/loading.gif" />', css: { backgroundColor: 'transparent', border: 'none' } });

        $.ajax({
            url: "../api/Account/Logout",
            type: "POST",
            dataType: "json",
            // code to run if the request succeeds;
            // the response is passed to the function
            success: function (json) {
                $("#divResult").html(json.html);
                window.location = "../UserPages/UserLogin.html";
            },
            // code to run if the request fails; the raw request and
            // status codes are passed to the function
            error: function (xhr, status) {
                alert("Sorry, there was a problem!");
            },
            // code to run regardless of success or failure
            complete: function (xhr, status) {
                //alert("The request is complete!");
            }
        });
    });
}

function checkForUserAdmin() {
    $.getJSON("../api/Account/AuthorizeUser")
        .done(function (data) {
            if (data.authorized) {

                $("#createUserLink").show();
                $("#mediaLink").show();
                $("#manageUserLink").show();
                $("#encodingLink").show();
            } else {
                $("#createUserLink").hide();
                $("#manageUserLink").hide();
                $("#mediaLink").hide();
                $("#encodingLink").hide();
                var str = window.location;
                if (str.toString().toUpperCase().indexOf("USERPAGES/HOME.HTML") == -1 && str.toString().toUpperCase().indexOf("USERPAGES/CHANGEPASSWORD.HTML") == -1) {

                    window.location = "../UserPages/UnAuthorizedAccess.html";
                }
                if (data.user == "Invalid") {
                    $.gritter.add({
                        title: 'Information!',
                        text: data.message,
                        sticky: false,
                        time: 2000,
                        before_close: function (e, manual_close) {
                            window.location = "../UserPages/UserLogin.html";
                        },
                    });
                }
                else { }
            }
            $("#loggedUser").text(data.user);
        }).error(function (request, status, error) {
            alert(request.responseText);
        });
}

function checkForUserLoggedIn() {
    $.getJSON("../api/Account/CheckUserLogIn")
        .done(function (data) {

            if (data.LoginStatus) {
                $("#loggedUser").text(data.user);
            } else {

                $.gritter.add({
                    title: 'Information!',
                    text: data.message,
                    sticky: false,
                    time: 3000,
                    before_close: function (e, manual_close) {
                        window.location = "../UserPages/UserLogin.html";
                    },
                });
            }

        }).error(function (request, status, error) {
            alert(request.responseText);
        });
}

$(document).ready(function () {
    $.unblockUI();
    $(document).ajaxStop($.unblockUI);
    $(document).ajaxStart($.blockUI({ message: '<img src="/images/loading.gif" />', css: { backgroundColor: 'transparent', border: 'none' } }));
    $(document).ajaxError($.unblockUI);
    
});

function htmlEncode(value) {
    //create a in-memory div, set it's inner text(which jQuery automatically encodes)
    //then grab the encoded contents back out.  The div never exists on the page.
    return $('<div/>').text(value).html();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}