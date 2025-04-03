const showPassword = (showPasswordId, passwordFieldId) => {
    const showPasswordTag = document.querySelector(showPasswordId);
    const passwordFieldTag = document.querySelector(passwordFieldId)

    showPasswordTag.addEventListener("click", function () {
        this.classList.toggle("fa-eye");
        this.classList.toggle("fa-eye-slash");
        const type = passwordFieldTag.getAttribute("type") === "password" ? "text" : "password"
        passwordFieldTag.setAttribute("type", type);
    })
}