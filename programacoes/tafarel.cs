float ultimoErro = 0;

Action pidFollower = () => {
    float velPadrao = 75;

    float kp = 8;
    float ki = 0;
    float kd = 8;

    float erro = bc.lightness (0) - bc.lightness (1);

    float p = kp * erro;
    float i = 0;
    if (i < 2) {
        i += ki * erro;
    }
    float d = kd * (erro - ultimoErro);

    var pid = p + i + d;
    bc.onTF (velPadrao + pid, velPadrao - pid);

    ultimoErro = erro;
};

Action<string> curva = (direcao) => {

    var velRotacao = direcao == "DIREITA" ? -500 : 500;

    bc.onTF (80, 80);
    bc.wait (500);
    while (bc.returnColor (0) == "BRANCO" && bc.returnColor (1) == "BRANCO") {
        bc.onTF (velRotacao, -velRotacao);
    }
    while (bc.returnColor (direcao == "DIREITA" ? 0 : 1) != "BRANCO") {
        bc.onTF (velRotacao, -velRotacao);
    }

};

Action verificaCurva = () => {
    if (bc.returnColor (0) == "VERDE") {
        curva ("DIREITA");
    } else if (bc.returnColor (1) == "VERDE") {
        curva ("ESQUERDA");
    } else if (bc.returnColor (0) == "PRETO" && bc.returnColor (1) == "PRETO") {

    }
};

Action tentaCurva = () => {
    var velRotacao = -500;

    bc.onTF (80, 80);
    bc.wait (500);
    var tempoAtual = bc.timer ();
    while (bc.returnColor (0) == "BRANCO" && bc.returnColor (1) == "BRANCO" && tempoAtual < 2000) {
        bc.onTF (velRotacao, -velRotacao);
    }
    while (bc.returnColor (0) != "BRANCO" && tempoAtual < 2000) {
        bc.onTF (velRotacao, -velRotacao);
    }
    if (tempoAtual > 2000) {
        tempoAtual = bc.timer ();
        while (bc.returnColor (0) == "BRANCO" && bc.returnColor (1) == "BRANCO" && tempoAtual < 2000) {
            bc.onTF (-velRotacao, velRotacao);
            bc.wait (1000);
        }
        while (bc.returnColor (0) != "BRANCO" && tempoAtual < 2000) {
            bc.onTF (-velRotacao, velRotacao);
            bc.wait (1000);
        }
    }
};

bc.actuatorSpeed (1500);
bc.actuatorUp (500);

while (true) {
    bc.printLCD (1, bc.returnColor (0));
    bc.printLCD (2, bc.returnColor (1));
    verificaCurva ();
    pidFollower ();
}