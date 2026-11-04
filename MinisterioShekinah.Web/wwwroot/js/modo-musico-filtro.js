document.addEventListener("DOMContentLoaded", function () {
    let selecionadas = JSON.parse(localStorage.getItem("musicasMissa") || "[]");
    const cards = document.querySelectorAll(".card-altar-musica");
    const aviso = document.getElementById("avisoFiltro");

    if (selecionadas.length > 0) {
        if (aviso) aviso.classList.remove("d-none");
        cards.forEach(card => {
            let id = card.getAttribute("data-id");
            if (!selecionadas.includes(id)) {
                card.style.display = "none";
            }
        });
    }
});