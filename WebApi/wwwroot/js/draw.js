async function get_images(image_load_callback) {
    var animateInfo = await hubConnection.invoke("GetAnimateInfo");

    var paths = animateInfo.mapCharToSprite;

    var images = {};

    var test = '';

    for (const [key, value] of Object.entries(paths)) {
        images[key] = new Image();
        images[key].src = value;
        test = key;
    }

    images[test].onload = image_load_callback; // draw when only last image loaded
   
    return images;
}

async function draw_board(canvas, data, maxw = 500, maxh=500) {

    if (window.images === undefined) {
        window.images = await get_images(() => draw_board(canvas, data, maxw, maxh));
        return;
    }

    var images = window.images;
    var rows = data.map.split('\n');
    var size = 32;

    var w = Math.max(...rows.map(v => v.length)) * size;
    var h = rows.length * size;

    var sw = maxw / w;
    var sh = maxh / h;
    var scale = Math.min(sw, sh);

    canvas.width = w * scale;
    canvas.height = h * scale;

    var ctx = canvas.getContext('2d');

    ctx.fillStyle = "rgb(0,0,0)";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.scale(scale, scale);

    for (var y = 0; y < rows.length; y++) {
        var row = rows[y];
        for (var x = 0; x < row.length; x++) {
            var image = images[row[x]];
            if (image === undefined) continue;
            ctx.drawImage(image, x * size, y * size);
        }
    }

    ctx.fillStyle = "rgb(250,250,250)";
    var snakes = data.gameState.snakes;
    for (var i = 0; i < snakes.length; i++) {
        var snake = snakes[i];
        var pos = snake.headPosition;
        var playerName = snake.playerOwner.name;
        ctx.fillText(playerName, pos.x * size, pos.y * size);
    }
}