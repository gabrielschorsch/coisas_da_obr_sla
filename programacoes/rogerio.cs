int random = 0;
var velPadrao = 300;
var velRotacao = 300;

/* #region FUNÇÕES DE MOVIMENTO */
Action<float, float> pid = (vel, kp) => {
    /* 
    Calcula o erro, que é a diferença entre os  valuees dos sensores da direita e os da esquerda.
    OBS: o value de 1.3 é arbitrário e refere-se à maior relevância dos sensores da ponta em relação aos centrais em relação ao movimento do motor.
     */
    var erro = ((bc.lightness (1) + bc.lightness (0) * 1.3)) - ((bc.lightness (3) + bc.lightness (4) * 1.3));
    // Aplica a variação do erro ao motor - casting do float apenas para conversão do value.
    bc.onTF ((float) (vel + (erro * kp)), (float) (vel - (erro * kp)));
};

Action curvaDireita = () => {
    /*
    Padroniza as referências visuais, mostrando no console a mensagem referente à direção e acendendo um led mostrando a entrada na rotina
    */
    bc.turnLedOn (255, 0, 0);
    bc.printLCD (2, "curva direita");
    // adianta o robô para fazer o giro em seu próprio eixo
    bc.onTF (40, 40);
    bc.wait (1500);
    // Verifica se o sensor central do robô já está na linha 
    while (bc.returnColor (2) == "PRETO" || (bc.returnColor (2) == "BRANCO" && bc.returnColor (3) == "PRETO")) {
        // Caso esteja, ele fará o giro até sair da linha
        bc.onTF (-30, 30);
    }
    // Realiza o giro enquanto o sensor central não reconhece a linha preta, além de verificar se os sensores centrais não estão na linha
    while (bc.returnColor (2) != "PRETO" && (bc.returnColor (1) == "BRANCO" || bc.returnColor (3) == "BRANCO")) {
        bc.turnLedOn (0, 255, 0);
        bc.onTF (-velRotacao, velRotacao);
    }
};

Action curvaEsquerda = () => {
    /*
    Padroniza as referências visuais, mostrando no console a mensagem referente à direção e acendendo um led mostrando a entrada na rotina
    */
    bc.turnLedOn (255, 0, 0);
    bc.printLCD (2, "curva esquerda");
    // adianta o robô para fazer o giro em seu próprio eixo
    bc.onTF (40, 40);
    bc.wait (1500);
    // Verifica se o sensor central do robô já está na linha 
    while (bc.returnColor (2) == "PRETO" || (bc.returnColor (2) == "BRANCO" && bc.returnColor (1) == "PRETO")) {
        // Caso esteja, ele fará o giro até sair da linha
        bc.onTF (30, -30);
    }
    // Realiza o giro enquanto o sensor central não reconhece a linha preta, além de verificar se os sensores centrais não estão na linha
    while (bc.returnColor (2) != "PRETO" && (bc.returnColor (1) == "BRANCO" || bc.returnColor (3) == "BRANCO")) {
        bc.turnLedOn (0, 255, 0);
        bc.onTF (velRotacao, -velRotacao);
    }
};

Action desviar = () => {
    while (bc.distance (0) >= 15) {
        bc.printLCD (2, $"{bc.distance(0)} - {bc.distance(1)}");
        bc.onTF (10, 10);
        bc.turnLedOn (255, 255, 255);
    }
    bc.turnLedOn (0, 0, 0);
    bc.onTF (0, 0);
    // Recupera o value em graus da direção do robô em relação ao norte da mesa
    var currentDegree = bc.compass ();
    // Verifica se o value está dentro do espectro possível
    if (currentDegree - 75 <= 1) {
        // Caso não esteja, faz a conversão
        currentDegree = 360 + currentDegree;
    }
    // Dado o value da direção recuperado anteriormente, faz uma curva até diminuir 75° graus da direção atual, com possibilidade de correção caso possua um erro maior que 3°
    while (bc.compass () % 90 <= 3 || bc.compass () >= currentDegree - 75) {
        // Imprime o value da direção no console
        bc.printLCD (2, bc.compass ().ToString ());
        // Realiza a rotação
        bc.onTF (velRotacao, -velRotacao);
    }
    // Adianta-se um pouco para desviar do objeto
    bc.onTF (20, 20);
    bc.wait (100);
    // Faz a rotação em volta do obstáculo
    while (bc.returnColor (2) != "PRETO") {
        bc.onTF (-50, 100);
    }
    // Adianta-se para alinhar-se futuramente
    bc.onTF (50, 50);
    bc.wait (300);
    // Realiza a curva
    curvaEsquerda ();
    // Espera o contato do sensor de toque para alinhar-se e seguir seu rumo
    while (!bc.touch (0)) {
        bc.onTF (-50, -50);
    }

};

/* #endregion */

/* #region FUNÇÕES CONTÍNUAS*/
Action resgate = () => {
    // ABAIXA O ATUADOR PARA PERMITIR A PASSAGEM PELO PORTÃO
    if (bc.inclination () > 10 && bc.inclination () < 355) {
        bc.onTF (-15, -15);
        bc.actuatorDown (4000);
    }
    // IDENTIFICA A ENTRADA NA RAMPA E SEGUE A LINHA 
    while (bc.inclination () > 10 && bc.inclination () < 355) {
        pid (velPadrao, 6);
        bc.printLCD (2, "Rampa");
        bc.turnLedOn (0, 0, 0);
    }
    // UMA VEZ QUE CHEGOU NO TOPO DA RAMPA, ANDA PARA FRENTE PARA NÃO CORRER O RISCO DE CAIR DE VOLTA
    bc.onTF (10, 10);
    bc.wait (1000);
    while (true) {
        //TODO: DEFINIR A ESTRATÉGIA DE RESGATE
        bc.onTF (50, 50);
    }
};

Action lineFollower = () => {
    bc.printLCD (1, "Seguindo linha");
    if (bc.returnColor (1) == "PRETO" && bc.returnColor (4) == "PRETO") {
        bc.turnLedOn (120, 120, 120);
        bc.printLCD (2, "encruzilhada");
        bc.onTF (velPadrao, velPadrao);
        bc.wait (500);
    } else if ((bc.returnColor (1) == "PRETO" && bc.returnColor (0) == "PRETO") || (bc.returnColor (1) == "VERDE" && bc.returnColor (3) != "VERDE")) {
        curvaDireita ();
    } else if ((bc.returnColor (3) == "PRETO" && bc.returnColor (4) == "PRETO") || (bc.returnColor (1) != "VERDE" && bc.returnColor (3) == "VERDE")) {
        curvaEsquerda ();
    } else {
        bc.turnLedOn (0, 0, 255);
        bc.printLCD (2, "pid");
        pid (velPadrao, 8);
    }

};

/* #endregion */

/* #region ROTINA DE EXECUÇÃO*/
bc.actuatorUp (3500);

while (true) {
    if ((bc.distance (2) < 30 ) && (bc.inclination () > 10 && bc.inclination () < 355)) {
        resgate ();
    }
    if ((bc.distance (0) < 15 && bc.distance (2) < 15)) {
        desviar ();
    }
    bc.printLCD (3, $"{ bc.returnColor(0) } - {bc.returnColor(1) } - {bc.returnColor(2)} - {bc.returnColor(3) } - {bc.returnColor(4) }");
    lineFollower ();
}

/* #endregion */