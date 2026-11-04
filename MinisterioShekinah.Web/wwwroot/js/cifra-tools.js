(function () {
    const notasSustenidos = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];
    const notasBemoes = ["C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B"];
    const mapaNotas = {
        "DB": "C#", "EB": "D#", "GB": "F#", "AB": "G#", "BB": "A#",
        "DO": "C", "RE": "D", "MI": "E", "FA": "F", "SOL": "G", "LA": "A", "SI": "B"
    };
    let semitonesAtuais = 0;
    let capotrasteAtual = 0;
    let cifraOriginalTexto = "";
    let scrollInterval = null;
    let scrollSpeed = 2;

    function inicializar() {
        const cifraElement = document.getElementById("cifraConteudo");
        const capoSelect = document.getElementById("capoSelect");

        if (cifraElement && cifraElement.innerText.trim() !== "") {
            cifraOriginalTexto = cifraElement.innerText;
            capotrasteAtual = parseInt(capoSelect ? capoSelect.value : "0");
            atualizarCifra();
        } else {
            // Se o conteúdo ainda estiver a carregar, tenta novamente em breve
            setTimeout(inicializar, 50);
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        inicializar();

        document.getElementById("btnTomMenos")?.addEventListener("click", () => mudarTom(-1));
        document.getElementById("btnTomMais")?.addEventListener("click", () => mudarTom(1));
        document.getElementById("btnTomReset")?.addEventListener("click", () => resetarTom());

        const capoSelect = document.getElementById("capoSelect");
        if (capoSelect) {
            capoSelect.addEventListener("change", function () {
                capotrasteAtual = parseInt(this.value || "0");
                atualizarCifra();
            });
        }

        document.getElementById("btnFonteMenos")?.addEventListener("click", () => alterarFonte(-1));
        document.getElementById("btnFonteMais")?.addEventListener("click", () => alterarFonte(1));
        document.getElementById("btnPlayRolagem")?.addEventListener("click", () => toggleRolagem());
        document.getElementById("btnVelocidadeMais")?.addEventListener("click", () => ajustarVelocidade(1));
        document.getElementById("btnVelocidadeMenos")?.addEventListener("click", () => ajustarVelocidade(-1));
    });

    function calcularTransposicaoNota(notaBaseInput, semitones) {
        let notaBase = notaBaseInput.toUpperCase();
        if (mapaNotas[notaBase]) {
            notaBase = mapaNotas[notaBase];
        }
        let index = notasSustenidos.indexOf(notaBase);
        if (index === -1) index = notasBemoes.indexOf(notaBase);
        if (index === -1) return notaBaseInput;
        let novoIndex = (index + semitones) % 12;
        if (novoIndex < 0) novoIndex += 12;
        return notasSustenidos[novoIndex];
    }

    function ehAcordeValido(tokenLimpo) {
        if (tokenLimpo.length > 10) return false;
        let partes = tokenLimpo.split('/');
        let principal = partes[0];
        let baixo = partes[1];
        let matchP = principal.match(/^([A-G][b#]?)(.*)$/i);
        if (!matchP) return false;
        let sufP = matchP[2];
        const regexSufixoValido = /^(?:m|maj|min|dim|aug|sus|add|\+|\-|°|0|[0-9#bM])*$/i;
        if (sufP && !regexSufixoValido.test(sufP)) return false;
        if (baixo) {
            let matchB = baixo.match(/^([A-G][b#]?)(.*)$/i);
            if (!matchB) return false;
            let sufB = matchB[2];
            if (sufB && !regexSufixoValido.test(sufB)) return false;
        }
        return true;
    }

    function transporAcorde(token, semitones) {
        if (!token) return "";
        let temColchetes = token.startsWith('[') && token.endsWith(']');
        let limpo = temColchetes ? token.substring(1, token.length - 1) : token;
        let partes = limpo.split('/');
        let principal = partes[0];
        let baixo = partes[1];
        let matchPrincipal = principal.match(/^([A-G][b#]?)(.*)$/i);
        if (!matchPrincipal) return token;
        let notaBase = matchPrincipal[1];
        let sufP = matchPrincipal[2];
        if (sufP === "0") sufP = "°";
        let novaNotaPrincipal = calcularTransposicaoNota(notaBase, semitones);
        let resultado = novaNotaPrincipal + sufP;
        if (baixo) {
            let matchBaixo = baixo.match(/^([A-G][b#]?)(.*)$/i);
            if (matchBaixo) {
                let notaBaixo = matchBaixo[1];
                let sufB = matchBaixo[2];
                if (sufB === "0") sufB = "°";
                let novaNotaBaixo = calcularTransposicaoNota(notaBaixo, semitones);
                resultado += '/' + novaNotaBaixo + sufB;
            } else {
                resultado += '/' + baixo;
            }
        }
        return temColchetes ? `[${resultado}]` : resultado;
    }

    function atualizarCifra() {
        const cifraElement = document.getElementById("cifraConteudo");
        if (!cifraElement || !cifraOriginalTexto) return;
        let deslocamentoTotal = semitonesAtuais + capotrasteAtual;
        let linhas = cifraOriginalTexto.split('\n');
        let linhasProcessadas = linhas.map(linha => {
            return linha.split(/(\s+)/).map(token => {
                if (!token.trim()) return token;
                let tokenLimpo = token.startsWith('[') && token.endsWith(']') ? token.substring(1, token.length - 1) : token;
                if (token.startsWith('[') && token.endsWith(']')) {
                    return transporAcorde(token, deslocamentoTotal);
                }
                if (ehAcordeValido(tokenLimpo)) {
                    return transporAcorde(token, deslocamentoTotal);
                }
                return token;
            }).join('');
        });
        cifraElement.innerText = linhasProcessadas.join('\n');
        const exibeTom = document.getElementById("tomAtualExibicao");
        const tomOriginalInput = document.getElementById("tomOriginal");
        let tomOriginal = (tomOriginalInput && tomOriginalInput.value) ? tomOriginalInput.value : "G";
        if (exibeTom) {
            let tomCalculado = transporAcorde(tomOriginal.trim(), deslocamentoTotal);
            exibeTom.innerText = tomCalculado.toUpperCase();
        }
    }

    function mudarTom(passos) {
        semitonesAtuais += passos;
        atualizarCifra();
    }

    function resetarTom() {
        semitonesAtuais = 0;
        capotrasteAtual = 0;
        const capoSelect = document.getElementById("capoSelect");
        if (capoSelect) capoSelect.value = "0";
        atualizarCifra();
    }

    function alterarFonte(direcao) {
        const cifraElement = document.getElementById("cifraConteudo");
        if (!cifraElement) return;
        let estiloAtual = window.getComputedStyle(cifraElement).fontSize;
        let tamanhoAtual = parseFloat(estiloAtual);
        let novoTamanho = tamanhoAtual + (direcao * 2);
        if (novoTamanho >= 12 && novoTamanho <= 32) {
            cifraElement.style.fontSize = novoTamanho + "px";
        }
    }

    function toggleRolagem() {
        const btn = document.getElementById("btnPlayRolagem");
        if (scrollInterval) {
            clearInterval(scrollInterval);
            scrollInterval = null;
            if (btn) btn.innerHTML = '<i class="bi bi-play-fill"></i> Rolagem';
        } else {
            scrollInterval = setInterval(() => {
                window.scrollBy(0, scrollSpeed);
                if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight) {
                    toggleRolagem();
                }
            }, 50);
            if (btn) btn.innerHTML = '<i class="bi bi-pause-fill"></i> Pausar';
        }
    }

    function ajustarVelocidade(delta) {
        scrollSpeed += delta;
        if (scrollSpeed < 1) scrollSpeed = 1;
        if (scrollSpeed > 10) scrollSpeed = 10;
        const indicador = document.getElementById("velocidadeIndicador");
        if (indicador) indicador.innerText = scrollSpeed + "x";
    }
})();