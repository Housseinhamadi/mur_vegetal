using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;
using System.Timers;

public class Interface : MonoBehaviour
{

    SerialPort serial1;
    SerialPort sp = new SerialPort("COM7", 9600);
    int luminosite;
    double temperature;
    ulong tempsHumidite;

    public Text LuminositeText;
    public Text TemperatureText;
    public Text HumiditeText;
    public Text LuminositeStatutText;
    public Text TemperatureStatutText;
    public Text HumiditeStatutText;

    void Start()
    {
        serial1 = new SerialPort();
        serial1.PortName = "COM7";
        serial1.Parity = Parity.None;
        serial1.BaudRate = 9600;
        serial1.DataBits = 8;
        serial1.StopBits = StopBits.One;
 
    }

    void Update()
    {
        string lu = ValeurLue();
        if(lu[0] == 'L') { // C'est la luminosité qui est lue
            luminosite = ParsingLuminosite(lu);
            AffichageLuminosite(luminosite);
            AffichageStatutLuminosite(luminosite);
        }
        else if (lu[0] == 'T') { // C'est la temperature qui est lue
            temperature = ParsingTemperature(lu);
            AffichageTemperature(temperature);
            AffichageStatutTemperature(temperature);
        }
        else if (lu[0] == 'H') { // C'est l'humidité qui est lue
            tempsHumidite = ParsingHumidite(lu);
            AffichageHumidite(tempsHumidite);
            AffichageStatutHumidite(tempsHumidite);
        }
        else {
            AffichageLuminosite(0);
            AffichageStatutLuminosite(0);
            AffichageTemperature(0);
            AffichageStatutTemperature(0);
            AffichageHumidite(0);
            AffichageStatutHumidite(0);
        }
        
    }

    /*
    Fonction qui reçoit la valeur de la luminosité en pourcentage et l'affiche dans le champs de texte
    qui convient (LuminositeText) 
    */
    void AffichageLuminosite(int luminosite) 
    {
        string chaine = luminosite.ToString();
        chaine += " %";
        LuminositeText.text = chaine;
    }

    /*
    Fonction qui reçoit la valeur de la temperature en °C et l'affiche dans le champs de texte
    qui convient (TemperatureText) 
    */
    void AffichageTemperature(double temperature) 
    {
        string chaine = temperature.ToString();
        chaine += " °C";
        TemperatureText.text = chaine;
    }

    /*
    Fonction qui reçoit le nombre de millisecondes écoulées depuis le dernier arrosage et l'affiche
    dans le champs de texte qui convient (HumiditeText) 
    */
    void AffichageHumidite(ulong timehumidite) 
    {
        ulong heures = ((timehumidite / 1000) / 60) / 60;
        //Debug.Log(timehumidite);
        ulong minutes = ((timehumidite / 1000) / 60) % 60;
        ulong secondes = (timehumidite / 1000) % 60;

        string chaine = "" + heures.ToString() + "h " + minutes.ToString() + "min " + secondes.ToString() + "sec";
        HumiditeText.text = chaine;
    }

    /*
    Fonction qui reçoit la valeur de la luminosité en pourcentage puis détermine le statut correspondant à cette
    valeur avant de l'affiche dans le champs de texte qui convient (LuminositeStatutText) 
    */
    void AffichageStatutLuminosite(int luminositeLu) 
    {
        string chaine = "";
        if(luminositeLu > 80) {
            chaine = "Trés élevée";
        }
        else if((luminositeLu > 60) && (luminositeLu <= 80)) {
            chaine = "Élevée";
        }
        else if((luminositeLu > 40) && (luminositeLu <= 60)) {
            chaine = "Normale";
        }
        else if((luminositeLu > 20) && (luminositeLu <= 40)) {
            chaine = "Basse";
        }
        else if((luminositeLu > 0) && (luminositeLu <= 20)) {
            chaine = "Trés Basse";
        }
        else {
            chaine = "Inconnue";
        }

        LuminositeStatutText.text = chaine;

    }

    /*
    Fonction qui reçoit la valeur de la temperature en °C puis détermine le statut correspondant à cette
    valeur avant de l'affiche dans le champs de texte qui convient (TemperatureStatutText) 
    */
    void AffichageStatutTemperature(double temperatureLu) 
    {
        string chaine = "";
        if(temperatureLu > 45.00) {
            chaine = "Trés élevée";
        }
        else if((temperatureLu > 35.00) && (temperatureLu <= 45.00)) {
            chaine = "Élevée";
        }
        else if((temperatureLu > 15.00) && (temperatureLu <= 35.00)) {
            chaine = "Normale";
        }
        else if((temperatureLu > 5.00) && (temperatureLu <= 15.00)) {
            chaine = "Basse";
        }
        else if (temperatureLu <= 5.00) {
            chaine = "Trés Basse";
        }
        else {
            chaine = "Inconnue";
        }

        TemperatureStatutText.text = chaine;

    }

    /*
    Fonction qui reçoit le nombre de millisecondes écoulées depuis le dernier arrosage puis détermine
    le statut correspondant avant de l'afficher dans le champs de texte qui convient (HumiditeStatutText) 
    */
    void AffichageStatutHumidite(ulong timehumidite) 
    {
        string chaine = "";
        // Temps restant superieur à 7h
        if(timehumidite > 25200000) {
            chaine = "Arrosage imminante";
        }
        // Temps restant entre 5h et 7h
        else if((timehumidite > 18000000) && (timehumidite <= 25200000)) {
            chaine = "Arroser bientôt";
        }
        // Temps restant entre 3h et 5h
        else if((timehumidite > 10800000) && (timehumidite <= 18000000)) {
            chaine = "A mi-chemin";
        }
        // Temps restant entre 1h et 3h
        else if((timehumidite > 3600000) && (timehumidite <= 10800000)) {
            chaine = "Encore trop tôt";
        }
        // Temps restant inferieur à 1h
        else if (timehumidite <= 3600000) {
            chaine = "Très humide";
        }
        else {
            chaine = "Inconnue";
        }

        HumiditeStatutText.text = chaine;

    }
    
    /*
    Cette fonction reçoit en parametre une chaine de caracteres de type L°°/L où °° represente la valeur
    de la luminosite. Cette fonction recupere la valeur °° pour la transformer en entier avant de la renvoyer
    */
    int ParsingLuminosite(string valeur)
    {
        string destination = valeur;
        destination = destination.Remove(destination.Length - 2);
        destination = destination.Remove(0,1);
        int result = int.Parse(destination);
        return result;
    }


    /*
    Cette fonction reçoit en parametre une chaine de caracteres de type T°°°°°/T où °°°°° represente la valeur 
    flottante de la température. Cette fonction recupere la valeur °°°°° pour la transformer en flottant avant
    de la renvoyer
    */
    double ParsingTemperature(string valeur)
    {
        string destination = valeur;
        destination = destination.Remove(destination.Length - 2);
        destination = destination.Remove(0,1);
        destination = destination.Replace('.',',');
        double result = double.Parse(destination);
        return result;
    }

    /*
    Cette fonction reçoit en parametre une chaine de caracteres de type H°°°°°°°°°/H où °° represente le nombre
    de milliseconde écoulées depuis le dernier arrosage. Cette fonction recupere la valeur °°°°°°°°° pour la 
    transformer en entier long avant de la renvoyer
    */
    ulong ParsingHumidite(string valeur)
    {
        string destination = valeur;
        destination = destination.Remove(destination.Length - 2);
        destination = destination.Remove(0,1);
        ulong result = ulong.Parse(destination);
        return result;
    }


    /*
    Cette fonction lit les valeurs envoyés par l'arduino avant de les retourner sous forme de chaine de caracteres
    */
    string ValeurLue () {        
        string lu;
        serial1.Open();
        lu = serial1.ReadLine();
        serial1.Close();
        return lu;
    }

    /*
    Cette fonction est exécutée dés que le bouton "Plante arrosee" est appuyé. Elle envoie le caractere "1"
    vers l'arduino pour lui signifier que la plante a été arrosé. 
    */
    public void plante_arrosee() {
        serial1.Open();
        serial1.Write("1");
        serial1.Close();
    }

    /* Fonction qui permet de quitter l'application*/
    public void Quit () {
        Application.Quit();
    }
}
