var ultimoErro = 0.0f;
var i = 0.0f;
var counter = 0;

Action pid = () => {
    float velPadrao = 75;
    float kp = 15f;
    float ki = 0.00000001f;
    float kd = 5f;

    var erro = (bc.lightness (1) - bc.lightness (3));

    var p = erro * kp;
    if (i < 2) {
        i += erro * ki;
    }
    var d = kd * (erro - ultimoErro);

    var gain = p + i + d;

    bc.onTF ((velPadrao + gain), (velPadrao - gain));

};

Action desvia = () => {
    var carlos = bc.compass ();
    var timer = bc.timer ();
    while (bc.timer () < timer + 1840) {
        //virou a primeira
        bc.onTF (1000, -1000);
        bc.printLCD (1, "1");
    }
    timer = bc.timer ();
    while (bc.distance (1) > 700 && bc.timer () < timer + 1000) {
        //andou a primeira
        bc.onTF (150, 150);
        bc.printLCD (1, "2");
    }
    bc.wait (500);
    bc.onTF (0, 0);
    bc.wait (500);
    timer = bc.timer ();
    while (bc.compass () % 90 > 5 || bc.timer () < timer + 900) {
        // virou a segunda
        bc.onTF (-1000, 1000);
        bc.printLCD (1, "3");

    }
    timer = bc.timer ();
    while (bc.distance (1) > 700 || bc.timer () < timer + 750) {
        //andou a segunda
        bc.onTF (150, 150);
        bc.turnLedOn (255, 0, 0);
        bc.printLCD (1, "4");
    }
    bc.wait (750);
    bc.onTF (0, 0);
    bc.wait (500);
    timer = bc.timer ();
    while (bc.timer () < timer + 1840) {
        // virou a terceira
        bc.onTF (-1000, 1000);
        bc.turnLedOn (0, 255, 0);
        bc.printLCD (1, "5");
    }
    while (bc.returnColor (2) != "PRETO") {
        bc.onTF (150, 150);
    }
    bc.onTF(200,200);
    bc.wait (500);
    timer = bc.timer ();
    while (bc.timer () < timer + 1840) {
        bc.onTF (1000, -1000);
    }
    while(!bc.touch(0)){
        bc.onTF(-570, -570);
    }

};

Action<string> curva = (direcao) => {

    var velRotacao = direcao == "direita" ? -750 : direcao == "esquerda" ? 750 : 0;
    bc.onTF (150, 150);
    bc.wait (500);
    while (bc.returnColor (2) == "PRETO" && (bc.returnColor (direcao == "direita" ? 2 : 1) == "BRANCO")) {
        // Caso esteja, ele fará o giro até sair da linha
        bc.onTF (-velRotacao, velRotacao);
    }

    // Realiza o giro enquanto o sensor central não reconhece a linha preta, além de verificar se os sensores centrais não estão na linha
    while (bc.returnColor (2) != "PRETO" && (bc.returnColor (1) == "BRANCO" || bc.returnColor (3) == "BRANCO")) {
        bc.turnLedOn (0, 255, 0);
        bc.onTF (-velRotacao, velRotacao);
    }
};

Action identificaRampa = () => {
    if (bc.distance (1) > 30 && (bc.inclination () > 15 && bc.inclination () < 355)) {
        bc.actuatorSpeed (750);
        bc.actuatorDown (750);
        while (!(bc.inclination () < 355) || !(bc.inclination () > 15)) {
            bc.onTF (0, 0);
            bc.wait (15000);
        }
    }
};

Action segueLinha = () => {
    if (bc.distance (2) < 20) {
        desvia ();
    } else if (bc.returnColor (0)  == "PRETO" || bc.returnColor (0) == "VERDE" || bc.returnColor (1) == "VERDE") {
        curva ("esquerda");
    } else if (bc.returnColor (4) == "PRETO" || bc.returnColor (3) == "VERDE" || bc.returnColor (4) == "VERDE") {
        curva ("direita");
    } else {
        pid ();
    }
};

bc.actuatorSpeed (1500);
bc.actuatorUp (1500);
while (true) {
    identificaRampa ();
    bc.printLCD (1, bc.inclination ().ToString ());
    segueLinha ();
}