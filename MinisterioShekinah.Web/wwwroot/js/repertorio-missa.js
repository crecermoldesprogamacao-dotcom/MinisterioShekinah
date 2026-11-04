document.addEventListener("DOMContentLoaded", function () {
    const checkboxes = document.querySelectorAll(".select-missa");
    const contadorElem = document.getElementById("contadorSelecionadas");
    const textoContador = document.getElementById("textoContador");
    const barraMissa = document.getElementById("barraMissa");
    const btnLimpar = document.getElementById("limparSelecao");

    let selecionadas = JSON.parse(localStorage.getItem("musicasMissa") || "[]");

    checkboxes.forEach(chk => {
        if (selecionadas.includes(chk.value)) {
            chk.checked = true;
        }

        chk.addEventListener("change", function () {
            let atualizadas = JSON.parse(localStorage.getItem("musicasMissa") || "[]");
            if (this.checked) {
                if (!atualizadas.includes(this.value)) atualizadas.push(this.value);
            } else {
                atualizadas = atualizadas.filter(id => id !== this.value);
            }
            localStorage.setItem("musicasMissa", JSON.stringify(atualizadas));
            atualizarBarra(atualizadas.length);
        });
    });

    function atualizarBarra(qtd) {
        if (contadorElem) contadorElem.innerText = qtd;
        if (textoContador) textoContador.innerText = qtd + (qtd === 1 ? " música selecionada" : " músicas selecionadas");
        if (barraMissa) {
            if (qtd > 0) {
                barraMissa.classList.remove("d-none");
            } else {
                barraMissa.classList.add("d-none");
            }
        }
    }

    if (btnLimpar) {
        btnLimpar.addEventListener("click", function () {
            localStorage.removeItem("musicasMissa");
            checkboxes.forEach(chk => chk.checked = false);
            atualizarBarra(0);
        });
    }

    atualizarBarra(selecionadas.length);
});