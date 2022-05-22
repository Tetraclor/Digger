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
}

function show_error(msg) {
    alert(msg);
}