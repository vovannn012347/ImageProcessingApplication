//login tracker, logins user as anonymous if user is not logged in
//tracks expiration and automatically expires token if needed
var TokenTracker = (function () {
    var cookieName = "jwtToken";
    var expirationKey = "expiration";

    function getCookie(name) {
        var match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
        if (match) return match[2];
        return null;
    }

    function setCookie(name, value, days) {
        var expires = "";
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + value + expires + "; path=/";
    }

    return {
        setToken: function (tokenValue, daysUntilExpire) {
            setCookie(cookieName, tokenValue, daysUntilExpire);
            setCookie(cookieName + "_" + expirationKey, Date.now() + daysUntilExpire * 24 * 60 * 60 * 1000, daysUntilExpire);
        },

        getToken: function () {
            var expirationDate = getCookie(cookieName + "_" + expirationKey);
            if (expirationDate && Date.now() > parseInt(expirationDate)) {
                this.clearToken();
                return null;
            }
            return getCookie(cookieName);
        },

        hasToken: function () {
            return !!this.getToken();
        },

        clearToken: function () {
            setCookie(cookieName, "", -1);
            setCookie(cookieName + "_" + expirationKey, "", -1);
        },

        waitForToken: function (interval = 100) {
            return new Promise((resolve) => {
                if (this.hasToken()) {
                    resolve(); // Resolve the promise
                    return;
                }
                const intervalId = setInterval(() => {
                    if (this.hasToken()) {
                        clearInterval(intervalId);
                        resolve(); // No need to return the token, resolve empty 
                    }
                }, interval);
            });
        }
    };
})();

(function () {
    if (!TokenTracker.hasToken()) {
        $.post('/api/user/anonymous',
            function (response) {
                TokenTracker.setToken(response.token, 1);
                //document.cookie = "jwtToken=" + response.token;
            })
            .fail(function (xhr, status, error) {
                console.error('Error fetching data for anonymous token:', xhr.responseText);
            });
    }
    else {
        //console.log("Token not found");
    }
    
})();

function logout() {
    $.ajax({
        url: '/api/user/anonymous',
        type: 'POST',
        success: function (response) {
            TokenTracker.clearToken();
            TokenTracker.setToken(response.token, 1);
        },
        error: function (xhr, status, error) {
            TokenTracker.clearToken();
            console.error('error logging out:', xhr.responseText);
        }
    });
}
//document.addEventListener("DOMContentLoaded", function () {
//});