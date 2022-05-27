async function login() {
    var login = $('#login')[0].value;
    var password = $('#password')[0].value;

    var result = await fetch(`api/login?login=${login}&password=${password}`)
    var answer = await result.text();

    if (!result.ok) {
        show_error(answer);
        return;
    }

    location.reload();
}

async function reg() {
    var login = $('#login')[0].value;
    var password = $('#password')[0].value;

    var result = await fetch(`api/reg?login=${login}&password=${password}`)
    var answer = await result.text();

    if (!result.ok) {
        show_error(answer);
        return;
    }

    location.reload();
}

async function update_my() {
    var myPlayer = await hubConnection.invoke("GetMe");
    $('#user-name')[0].innerText = `${myPlayer.name}   ${myPlayer.rate}`;
    $('#token')[0].value = await hubConnection.invoke("GetMyToken");
}

function show_error(msg) {
    alert(msg);
}

async function copy_token(buttonElement) {
    var token = await hubConnection.invoke("GetMyToken");

    var showCopiedTextInButton = () => {
        console.log("Token Copied");
        if (buttonElement.innerText !== 'Скопировано!') {
            const originalText = buttonElement.innerText;
            buttonElement.innerText = 'Скопировано!';
            setTimeout(() => {
                buttonElement.innerText = originalText;
            }, 1500);
        }
    }

    if (token) {
        // Не работает на сервере, (локально работает)  возможно потому что нет https
        if (navigator.clipboard) {
            navigator.clipboard.writeText(token)
                .then(() => showCopiedTextInButton())
                .catch(err => {
                    console.log('Something went wrong', err);
                })
        } else {
            var inputElement = document.getElementById("token");

            /* Select the text field */
            inputElement.select();

            /* Copy the text inside the text field */
            document.execCommand("copy");

            showCopiedTextInButton()
        }
    }
}