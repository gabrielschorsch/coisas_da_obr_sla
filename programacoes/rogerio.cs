int random = 0;
var velPadrao = 300;
var velRotacao = 300;

/* #region FUNÇÕES DE MOVIMENTO */
Action<float, float> pid = (vel, kp) => {
    /* 
    #region [realiza_calculo]
    Calcula o erro, que é a diferença entre os  valuees dos sensores da direita e os da esquerda.
    OBS: o value de 1.3 é arbitrário e refere-se à maior relevância dos sensores da ponta em relação aos centrais em relação ao movimento do motor.
     */
    var erro = ((bc.lightness (1) + bc.lightness (0) * 1.3)) - ((bc.lightness (3) + bc.lightness (4) * 1.3));
    // #endregion
    // Aplica a variação do erro ao motor - casting do float apenas para conversão do value.
    // #region [mover]
    bc.onTF ((float) (vel + (erro * kp)), (float) (vel - (erro * kp)));
    // #endregion
};

Action curvaDireita = () => {
    /*
    Padroniza as referências visuais, mostrando no console a mensagem referente à direção e acendendo um led mostrando a entrada na rotina
    */

    // #region [identifica_curva]
    bc.turnLedOn (255, 0, 0);
    bc.printLCD (2, "curva direita");
    // #endregion

    // adianta o robô para fazer o giro em seu próprio eixo
    bc.onTF (40, 40);
    bc.wait (1500);
    // #region [realiza_curva] Verifica se o sensor central do robô já está na linha 
    while (bc.returnColor (2) == "PRETO" || (bc.returnColor (2) == "BRANCO" && bc.returnColor (3) == "PRETO")) {
        // Caso esteja, ele fará o giro até sair da linha
        bc.onTF (-30, 30);
    }
    // #endregion

    // #region [realiza_curva] Realiza o giro enquanto o sensor central não reconhece a linha preta, além de verificar se os sensores centrais não estão na linha
    while (bc.returnColor (2) != "PRETO" && (bc.returnColor (1) == "BRANCO" || bc.returnColor (3) == "BRANCO")) {
        bc.turnLedOn (0, 255, 0);
        bc.onTF (-velRotacao, velRotacao);
    }
    // #endregion
};

Action curvaEsquerda = () => {
    /*
    Padroniza as referências visuais, mostrando no console a mensagem referente à direção e acendendo um led mostrando a entrada na rotina
    */
    // #region [identifica_curva]
    bc.turnLedOn (255, 0, 0);
    bc.printLCD (2, "curva esquerda");
    // #endregion

    // adianta o robô para fazer o giro em seu próprio eixo
    bc.onTF (40, 40);
    bc.wait (1500);
    // #region [realiza_curva] Verifica se o sensor central do robô já está na linha 
    while (bc.returnColor (2) == "PRETO" || (bc.returnColor (2) == "BRANCO" && bc.returnColor (1) == "PRETO")) {
        // Caso esteja, ele fará o giro até sair da linha
        bc.onTF (30, -30);
    }
    // #endregion

    // #region [realiza_curva] Realiza o giro enquanto o sensor central não reconhece a linha preta, além de verificar se os sensores centrais não estão na linha
    while (bc.returnColor (2) != "PRETO" && (bc.returnColor (1) == "BRANCO" || bc.returnColor (3) == "BRANCO")) {
        bc.turnLedOn (0, 255, 0);
        bc.onTF (velRotacao, -velRotacao);
    }
    // #endregion
};

Action desviar = () => {
    // #region [aproximar_obstaculo] 
    while (bc.distance (0) >= 15) {
        bc.printLCD (2, $"{bc.distance(0)} - {bc.distance(1)}");
        bc.onTF (10, 10);
        bc.turnLedOn (255, 255, 255);
    }
    // #endregion

    bc.turnLedOn (0, 0, 0);
    bc.onTF (0, 0);
    // #region [realiza_calculo] Recupera o value em graus da direção do robô em relação ao norte da mesa
    var currentDegree = bc.compass ();
    // Verifica se o value está dentro do espectro possível
    if (currentDegree - 75 <= 1) {
        // Caso não esteja, faz a conversão
        currentDegree = 360 + currentDegree;
    }

    // #endregion

    // #region [giro_proprio_eixo]
    // Dado o value da direção recuperado anteriormente, faz uma curva até diminuir 75° graus da direção atual, com possibilidade de correção caso possua um erro maior que 3°
    while (bc.compass () % 90 <= 3 || bc.compass () >= currentDegree - 75) {
        // Imprime o value da direção no console
        bc.printLCD (2, bc.compass ().ToString ());
        // Realiza a rotação
        bc.onTF (velRotacao, -velRotacao);
    }
    // #endregion

    // #region [mover] Adianta-se um pouco para desviar do objeto
    bc.onTF (20, 20);
    bc.wait (100);

    // #endregion

    // #region [desvia_obstaculo] Faz a rotação em volta do obstáculo
    while (bc.returnColor (2) != "PRETO") {
        bc.onTF (-50, 100);
    }

    // #endregion

    // #region [mover] Adianta-se para alinhar-se futuramente
    bc.onTF (50, 50);
    bc.wait (300);
    // #endregion

    // #region [realiza_curva] Realiza a curva
    curvaEsquerda ();
    // #endregion

    // #region [aguarda_sensor]Espera o contato do sensor de toque para alinhar-se e seguir seu rumo
    while (!bc.touch (0)) {
        bc.onTF (-50, -50);
    }
    // #endregion

};

/* #endregion */

/* #region FUNÇÕES CONTÍNUAS*/
Action resgate = () => {
    // #region [move_atuador] ABAIXA O ATUADOR PARA PERMITIR A PASSAGEM PELO PORTÃO
    if (bc.inclination () > 10 && bc.inclination () < 355) {
        bc.onTF (-15, -15);
        bc.actuatorDown (4000);
    }
    // #endregion

    // #region [inclinacao] IDENTIFICA A ENTRADA NA RAMPA E SEGUE A LINHA 
    while (bc.inclination () > 10 && bc.inclination () < 355) {
        pid (velPadrao, 6);
        bc.printLCD (2, "Rampa");
        bc.turnLedOn (0, 0, 0);
    }
    // #endregion

    // #region [resgate] UMA VEZ QUE CHEGOU NO TOPO DA RAMPA, ANDA PARA FRENTE PARA NÃO CORRER O RISCO DE CAIR DE VOLTA
    bc.onTF (10, 10);
    bc.wait (1000);
    while (true) {
        //TODO: DEFINIR A ESTRATÉGIA DE RESGATE
        bc.onTF (50, 50);
    }
    // #endregion

};

Action lineFollower = () => {
    bc.printLCD (1, "Seguindo linha");
    if (bc.returnColor (1) == "PRETO" && bc.returnColor (4) == "PRETO") {
        // #region [encruzilhada]
        bc.turnLedOn (120, 120, 120);
        bc.printLCD (2, "encruzilhada");
        bc.onTF (velPadrao, velPadrao);
        bc.wait (500);
        // #endregion
    } else if ((bc.returnColor (1) == "PRETO" && bc.returnColor (0) == "PRETO") || (bc.returnColor (1) == "VERDE" && bc.returnColor (3) != "VERDE")) {
        // #region [realiza_curva]
        curvaDireita ();
        // #endregion
    } else if ((bc.returnColor (3) == "PRETO" && bc.returnColor (4) == "PRETO") || (bc.returnColor (1) != "VERDE" && bc.returnColor (3) == "VERDE")) {
        // #region [realiza_curva]
        curvaEsquerda ();
        // #endregion
    } else {
        // #region [mover]
        bc.turnLedOn (0, 0, 255);
        bc.printLCD (2, "pid");
        pid (velPadrao, 8);
        // #endregion
    }

};

/* #endregion */

/* #region ROTINA DE EXECUÇÃO*/
//#region [move_atuador]
bc.actuatorUp (3500);
//#endregion

while (true) {
    // #region [resgate]
    if ((bc.distance (2) < 30) && (bc.inclination () > 10 && bc.inclination () < 355)) {
        resgate ();
    }
    // #endregion

    // #region [desvia_obstaculo]
    if ((bc.distance (0) < 15 && bc.distance (2) < 15)) {
        desviar ();
    }
    // #endregion

    bc.printLCD (3, $"{ bc.returnColor(0) } - {bc.returnColor(1) } - {bc.returnColor(2)} - {bc.returnColor(3) } - {bc.returnColor(4) }");
    // #region [mover]
    lineFollower ();
    // #endregion
}

/* #endregion */