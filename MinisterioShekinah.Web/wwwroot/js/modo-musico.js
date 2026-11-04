document.addEventListener("DOMContentLoaded", function () {
    let timerRolagem = null;
    let velocidadeAtual = 1;

    const btnPlay = document.getElementById("btnPlayGlobal");
    const btnMenos = document.getElementById("btnVelocidadeMenos");
    const btnMais = document.getElementById("btnVelocidadeMais");
    const indicador = document.getElementById("velocidadeIndicador");

    if (btnPlay) {
        btnPlay.addEventListener("click", function () {
            if (timerRolagem !== null) {
                clearInterval(timerRolagem);
                timerRolagem = null;
                btnPlay.innerHTML = '<i class="bi bi-play-fill"></i> Rolagem Auto';
            } else {
                timerRolagem = setInterval(function () {
                    window.scrollBy(0, velocidadeAtual);
                    if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight - 5) {
                        clearInterval(timerRolagem);
                        timerRolagem = null;
                        btnPlay.innerHTML = '<i class="bi bi-play-fill"></i> Rolagem Auto';
                    }
                }, 40);
                btnPlay.innerHTML = '<i class="bi bi-pause-fill"></i> Pausar';
            }
        });
    }

    if (btnMais) {
        btnMais.addEventListener("click", function () {
            if (velocidadeAtual < 10) {
                velocidadeAtual++;
                if (indicador) indicador.innerText = velocidadeAtual + "x";
            }
        });
    }

    if (btnMenos) {
        btnMenos.addEventListener("click", function () {
            if (velocidadeAtual > 1) {
                velocidadeAtual--;
                if (indicador) indicador.innerText = velocidadeAtual + "x";
            }
        });
    }
});