$(document).ready(function () {
    // Lấy dữ liệu từ phần tử ẩn
    var images = $('#productImages').data('images');
    var mainImage = $('#mainImage');
    var currentIndex = 0;
    var changeImageInterval = 3000; // 3 seconds

    function changeMainImage(index) {
        mainImage.attr('src', images[index]);
        $('.thumbnail').removeClass('active');
        $('.thumbnail').eq(index).addClass('active');
    }

    // Auto change image every 3 seconds
    var interval = setInterval(function () {
        currentIndex = (currentIndex + 1) % images.length;
        changeMainImage(currentIndex);
    }, changeImageInterval);

    // Change image on thumbnail click
    $('.thumbnail').on('click', function () {
        clearInterval(interval); // Dừng slideshow khi người dùng click
        currentIndex = $('.thumbnail').index(this);
        changeMainImage(currentIndex);

        // Tiếp tục slideshow sau 3 giây kể từ lần click cuối
        interval = setInterval(function () {
            currentIndex = (currentIndex + 1) % images.length;
            changeMainImage(currentIndex);
        }, changeImageInterval);
    });
});
