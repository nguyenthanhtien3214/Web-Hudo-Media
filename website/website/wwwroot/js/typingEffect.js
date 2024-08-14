document.addEventListener('DOMContentLoaded', function () {
    const textElement = document.querySelector('.typing-text');
    const text = textElement.getAttribute('data-text').split("\n"); // Tách các đoạn văn bản theo ký tự xuống dòng
    let lineIndex = 0;
    let charIndex = 0;

    function type() {
        if (lineIndex < text.length) {
            if (charIndex < text[lineIndex].length) {
                textElement.innerHTML += text[lineIndex].charAt(charIndex);
                charIndex++;
                setTimeout(type, 30); // tốc độ đánh máy, có thể điều chỉnh thời gian này
            } else {
                textElement.innerHTML += '<br>'; // Thêm ngắt dòng khi hết một đoạn
                charIndex = 0;
                lineIndex++;
                setTimeout(type, 30);
            }
        }
    }

    type();
});
