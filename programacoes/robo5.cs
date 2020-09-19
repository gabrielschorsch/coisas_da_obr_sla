var ultimoErro = 0.0f;
var i = 0.0f;
var counter = 0;

Action pid = () => {
    var velPadrao = 55;
    var kp = 7f;
    var ki = 0.0000001f;
    var kd = 3f;
    var d = 0f;

    var erro = ((bc.lightness (0) + bc.lightness (1)) - (bc.lightness (3) + bc.lightness (4)));

    var p = erro * kp;
    if (ultimoErro != erro) {
        counter++;
        d = kd * (erro - ultimoErro);
    }
    if (i < 2) {
        i += (erro * ki);
    }
    var pid_val = p + i + d;

    // bc.printLCD (1, "p:" + p + " i: " + i + " d: " + d);
    // bc.printLCD (2, "erro: " + erro);

    // bc.printLCD (3, "ganho: " + (velPadrao + pid_val));

    // Aplica a variação do erro ao motor - casting do float apenas para conversão do value.
    bc.onTF ((float) (velPadrao + pid_val), (float) (velPadrao - pid_val));

    // bc.printLCD (1, d.ToString ());
    bc.printLCD (2, erro.ToString ());
    bc.printLCD (3, ultimoErro.ToString ());

    if (counter != 0) {
        bc.printLCD (1, counter.ToString ());
    }

    ultimoErro = erro;

};

Action<string> curva = (direcao) => {

    var velRotacao = direcao == "direita" ? -750 : direcao == "esquerda" ? 750 : 0;
    bc.onTF (150, 150);
    bc.wait (500);
    while (bc.returnColor (2) == "PRETO" && (bc.returnColor (direcao == "direita" ? 0 : 3) == "BRANCO" && bc.returnColor (direcao == "direita" ? 3 : 0) == "PRETO")) {
        // Caso esteja, ele fará o giro até sair da linha
        bc.onTF (-velRotacao, velRotacao);
    }

    // Realiza o giro enquanto o sensor central não reconhece a linha preta, além de verificar se os sensores centrais não estão na linha
    while (bc.returnColor (2) != "PRETO" && (bc.returnColor (1) == "BRANCO" || bc.returnColor (3) == "BRANCO")) {
        bc.turnLedOn (0, 255, 0);
        bc.onTF (-velRotacao, velRotacao);
    }
};

Action segueLinha = () => {
    if ((bc.returnColor (0) == "PRETO" && bc.returnColor (1) == "PRETO") || (bc.returnColor (0) == "VERDE" || bc.returnColor (1) == "VERDE")) {
        curva ("esquerda");
    } else if ((bc.returnColor (3) == "PRETO" && bc.returnColor (4) == "PRETO") || (bc.returnColor (3) == "VERDE" || bc.returnColor (4) == "VERDE")) {
        curva ("direita");
    } else {
        pid ();
    }
};

bc.actuatorSpeed (1500);
bc.actuatorUp (1500);
while (true) {
    segueLinha ();
}