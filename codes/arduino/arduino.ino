int pinTherm = 0; // pin du thermistor
int valTemperature; // valeur lue par le thermistor
int pinPR=A1; //pin de connexion pour la photorésistance
int valMin=0; // on initialise la valeur minimale au plus haut
int valMax=1024; // et la valeur maximale au plus bas
unsigned long lastCheck; // Moment du dernier arrosage
int pinLedLuminosite = 11; // pin du led de la luminosite
int pinLedHumidite = 12; // pin du led de l'humidite
int pinLedTemperature = 13; // pin du led de la temperatur

void setup() {
  Serial.begin(9600); //Initialisation de la communication avec la console
  lastCheck = millis(); // On suppose que la plante a été arrosé ao demarrage de l'application
  pinMode(pinLedLuminosite, OUTPUT);
  pinMode(pinLedTemperature, OUTPUT);
  pinMode(pinLedHumidite, OUTPUT);
}

/*
  Fonction qui recupere les valeur relevées par le thermistor. Ces valeurs sont d'abord
  converties en °C avant d'être envoyées vers l'application Unity.
  Si la température n'est pas comprise entre 15 et 45 °C, une Led rouge s'allume sur le montage. 
*/
void transmission_temperature() {
  valTemperature = analogRead(pinTherm);
  // This is OK
  double tempK = log(10000.0 * ((1024.0 / valTemperature - 1)));
  tempK = 1 / (0.001129148 + (0.000234125 + (0.0000000876741 * tempK * tempK )) * tempK );       //  Temp Kelvin
  float tempC = tempK - 273.15;           // Convert Kelvin to Celcius
  if ((tempC <= 15) || (tempC >= 45))
    digitalWrite(pinLedTemperature, HIGH);
  else
    digitalWrite(pinLedTemperature, LOW);

  Serial.print("T");
  Serial.print(tempC);
  Serial.println("/T");
}

/*
  Fonction qui recupere les valeur relevées par la photorésistance. Ces valeurs sont d'abord
  converties en pourcentage avant d'être envoyées vers l'application Unity.
  Si le taux de luminosité est inférieur à 33% , une Led bleue s'allume sur le montage. 
*/
void transmission_luminosite() {
  int valeur=analogRead(pinPR);
  int pourcentage=map(valeur,valMin,valMax,0,100);
  if (pourcentage <= 33)
    digitalWrite(pinLedLuminosite, HIGH);
  else 
    digitalWrite(pinLedLuminosite, LOW);  
  
  Serial.print("L");
  Serial.print(pourcentage);
  Serial.println("/L");
}

/*
  Fonction qui calcule le nombre de millisecondes écoulées depuis le dernierr arrosage.
  Le moment du dernier arrosage est enregistrée dans la variable lastCheck. A chaque
  que l'utilisateur appuie sur le bouton "Plante arrosée", la variable lastCheck est mis à 
  jour.
  Le nombre de millisecondes est envoyé à l'application Unity.
  Si le nombre de millisecondes écoulées est supérieur à 28800000 (8 heures), la Led verte
  est allumé.
  Enfin la fonction vérifie si l'utilisateur a arrosé la plante.
*/
void transmission_humidite() {
  // Nombre de milliseconde écoulée depuis le dernier arrosage
  unsigned long ms = millis()-lastCheck;
    Serial.print("H");
    Serial.print(ms);
    Serial.println("/H");
    if (ms >= 28800000) // huit heures de temps passées depuis le dernier arrosage 
      digitalWrite(pinLedHumidite, HIGH);
    else
      digitalWrite(pinLedHumidite, LOW);  

    if (is_watered())
        lastCheck = millis();  
}
 
/*
Fonction qui vérifie si l'utilisateur a arrosé la plante. En effet si l'utilisateur a 
arrosée la plante et appuyé sur le bouton "Plante arrosée", le caractére '1' est envoyé
par l'application Unity. Cette fonction écoute cette réponse de Unity. Si le 1 est reçu, la
fonction renvoie vrai sinon faux.
*/
bool is_watered() {
  bool etat = false;
  int reponse = 0;
  if(Serial.available() > 0) {
    reponse = Serial.read();
    if (reponse == '1')
      etat = true;
  }
  return etat;
}

void loop() {
  transmission_luminosite();
  delay(250);
  transmission_humidite();
  delay(250);
  transmission_temperature();
  delay(250);
}
