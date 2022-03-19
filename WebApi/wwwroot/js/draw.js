async function get_images() {
    var animateInfo = await hubConnection.invoke("GetAnimateInfo");

    var paths = animateInfo.mapCharToSprite;

    var images = {};

    for (const [key, value] of Object.entries(paths)) {
        images[key] = new Image();
        images[key].src = value;
    }

    return images;
}

async function draw_board(canvas, data, maxw = 500, maxh=500) {

    if (window.images === undefined) {
        window.images = await get_images();
    }

    var images = window.images;

    var ctx = canvas.getContext('2d');
    var size = 32;

    var rows = data.split('\n');

    var w = rows[0].length * size;
    var h = rows.length * size;

    var sw = maxw / w;
    var sh = maxh / h;

    canvas.width = Math.min(w, maxw);
    canvas.height = Math.min(h, maxh);

    ctx.fillStyle = "rgb(0,0,0)";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.scale(sw, sh);

    for (var y = 0; y < rows.length; y++) {
        var row = rows[y];
        for (var x = 0; x < row.length; x++) {
            var image = images[row[x]];
            if (image === undefined) continue;
            ctx.drawImage(image, x * size, y * size);
        }
    }
}