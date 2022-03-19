async function get_images() {

    var animateInfo = await hubConnection.invoke("GetAnimateInfo");
    console.log(animateInfo);

    var paths = animateInfo.mapCharToSprite;

    var images = {};

    for (const [key, value] of Object.entries(paths)) {
        images[key] = new Image();
        images[key].src = value;
    }

    return images;
}

async function draw_board(canvas, data) {
    // use the intrinsic size of image in CSS pixels for the canvas element
    
    if (window.images === undefined) {
        window.images = await get_images();
    }

    var images = window.images;
    var ctx = canvas.getContext('2d');
    var size = 32;

    var rows = data.split('\n');

    console.log(rows);

    canvas.width = rows[0].length * size;
    canvas.height = (rows.length) * size;

    ctx.fillStyle = "rgb(0,0,0)";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    for (var y = 0; y < rows.length; y++) {
        var row = rows[y];

        for (var x = 0; x < row.length; x++) {
            var image = images[row[x]];
            if (image === undefined) continue;
            ctx.drawImage(image, x * size, y * size);
        }
    }
}