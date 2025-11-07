using UnityEngine;
using UnityEngine.UI;

public class jumbotronTexts : MonoBehaviour
{

    public Text[] roundTexts;
    public Text[] timeTexts;
    public Text[] stateTexts;

    private bool timeTicking = false;
    private float actualTiming = 0f;

    public ParticleSystem confettiParticles;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowTexts(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (timeTicking){
            UpdateTimeJumbotron();
        }
    }

    public void Victory() {
        //Para cuando el jugador gana
        //ocultamos los demás textos y solo mostramos el victoria con fondo verde (tambien hay confeti)
        ShowTexts(2);
        confettiParticles.Play();
    }

    public void Loose() {
        // Para cuando el jugador pierde
        //ocultamos los demás textos y solo mostramos el derrota con fondo rojo
        ShowTexts(3);
    }

    public void ShowTexts(int textType) {
        //textTyoe: 1=tiempo, 2=victoria, 3=derrota
        switch (textType){
            case 1:
                //Mostramos el tiempo
                foreach(var textoTiempo in timeTexts) {
                    textoTiempo.enabled = true;
                }
                foreach(var textoEstado in stateTexts) {
                    textoEstado.enabled = false;
                }
                break;

            case 2:
                //Ocultamos tiempo y mostramos el texto de victoria
                foreach(var textoTiempo in timeTexts) {
                    textoTiempo.enabled = false;
                }
                foreach(var textoEstado in stateTexts) {
                    textoEstado.enabled = true;
                    textoEstado.text = "VICTORIA";
                }
                break;

            case 3:
                //Ocultamos tiempo y mostramos el texto de derrota
                foreach(var textoTiempo in timeTexts) {
                    textoTiempo.enabled = false;
                }
                foreach(var textoEstado in stateTexts) {
                    textoEstado.enabled = true;
                    textoEstado.text = "DERROTA";
                }
                break;
            case 7:
                //Ocultamos tiempo y mostramos el texto de derrota
                foreach(var textoTiempo in timeTexts) {
                    textoTiempo.enabled = false;
                    
                }
                foreach(var textoEstado in stateTexts) {
                    textoEstado.enabled = true;
                    textoEstado.text = "GO";
                }
                break;
        }
    }

    private string parseTimeToText(float timeOriginal) {
        if (timeOriginal < 0){
            return "";
        }
        // Separmos segundos y milésimas
        int segundos = Mathf.FloorToInt(timeOriginal) + 1;
        int milesimas = Mathf.FloorToInt((timeOriginal - segundos) * 100);
        return segundos.ToString();
        //return string.Format("{0:00}:{1:00}", segundos, milesimas);
    }

    public void SetTimeJumbotron(float time) {
        actualTiming = time;
        foreach(var timeTxt in timeTexts) {
            timeTxt.text = parseTimeToText(time);
        }
    }

    public void StartTimeJumbotron() {
        timeTicking = true;
    }
    public void StopTimeJumbotron() {
        timeTicking = false;
    }

    public void UpdateTimeJumbotron() {
        actualTiming -= Time.deltaTime;
        string text = parseTimeToText(actualTiming);
        foreach(var timeTxt in timeTexts) {
            timeTxt.text=text;
        }
    }

    public void SetRoundNumJumbotron(int roundNum) {
        foreach(var roundTxt in roundTexts) {
            roundTxt.text ="RONDA "+ roundNum.ToString() + "/3";
        }
    }
}
