// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Fix Selenium 3 + ChromeDriver 146 timing: submit form via sync XHR, write response directly
(function () {
    document.addEventListener('submit', function (e) {
        var form = e.target;
        var action = form.action || window.location.href;
        if (action.indexOf('/Identity/') !== -1) return;
        e.preventDefault();
        try {
            var xhr = new XMLHttpRequest();
            xhr.open(form.method || 'POST', action, false);
            var pairs = [];
            for (var i = 0; i < form.elements.length; i++) {
                var el = form.elements[i];
                if (!el.name || el.disabled) continue;
                if ((el.type === 'checkbox' || el.type === 'radio') && !el.checked) continue;
                if (el.type === 'submit' || el.type === 'image' || el.tagName === 'BUTTON') continue;
                if (el.type === 'select-one') {
                    if (el.selectedIndex >= 0) {
                        pairs.push(encodeURIComponent(el.name) + '=' + encodeURIComponent(el.options[el.selectedIndex].value));
                    }
                } else if (el.type === 'select-multiple') {
                    for (var j = 0; j < el.options.length; j++) {
                        if (el.options[j].selected) {
                            pairs.push(encodeURIComponent(el.name) + '=' + encodeURIComponent(el.options[j].value));
                        }
                    }
                } else {
                    pairs.push(encodeURIComponent(el.name) + '=' + encodeURIComponent(el.value));
                }
            }
            xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            xhr.send(pairs.join('&'));
            document.open();
            document.write(xhr.responseText);
            document.close();
        } catch (ex) {
            form.submit();
        }
    });
})();
