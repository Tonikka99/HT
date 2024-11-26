using System;
using System.Diagnostics.CodeAnalysis;
using Jypeli;


namespace RiddleGame;

/// @author Antturpe
/// @version 21.11.2024
/// <summary>
/// 
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")] // vammainen "_" ehdotus poistetu
public class RiddleGame : PhysicsGame
{
    private PhysicsObject _pelaaja;
    private Timer peliAjastin;
    private IntMeter aika;
    private IntMeter keratty;
    private bool task1 = false;
    private bool task2 = false;
    private string pelaajanVastaus = ""; //Vastaus 1715.
    private int oikeaKoodi = 1715; // Oikea koodi lukolle
    private bool tehtavaSuoritettu = false;


    public override void Begin()
    {
        AsetaTausta();
        LuoIntroKuva();

        _pelaaja = LuoPelaaja(Level.Left + 20, 0);
        AsetaOhjaimet(_pelaaja);
        LuoKello();

        LuoRajatutSeinat();

        //Luodaan ajastin ja laitetaan se näkyväksi 
        aika = new IntMeter(40);
        Label aikalabel = new Label();
        aikalabel.BindTo(aika);
        aikalabel.TextColor = Color.Red;
        aikalabel.Position = new Vector(0, Screen.Top - 40);
        Add(aikalabel);


        // Luodaan tarkistin joka tarkastaa aikaa ja lopettaa pelin jos aika loppuu
        peliAjastin = new Timer();
        peliAjastin.Interval = 1.0; // Laskee sekunteja
        peliAjastin.Timeout += VahennaAikaa;


        LuoAjastin(5.0, LuoKello);
        keratty = KeratytKellot();
        AddCollisionHandler(_pelaaja, "kello", TormaysKelloon);
        

        puzzle1(_pelaaja);
        LuoPuzzle2Lukko(40, 40, task1);
        puzzle2(_pelaaja, task1);
        LuoNaytto(task1);
        LuoLukkoJaTormays(100, 200);
    }


    /// <summary>
    /// Luodaan pelille background kuva
    /// </summary>
    private void AsetaTausta()
    {
        PhysicsObject tausta = new PhysicsObject(Level.Width, Level.Height);
        tausta.Y = Level.Center.Y;
        tausta.X = Level.Center.X;
        tausta.Image = LoadImage("karttaLayout");
        tausta.IgnoresCollisionResponse = true;
        Add(tausta, -1);

    }


    /// <summary>
    /// Luodaan aluksi kuva joka avaa pelin "tarinaa" ja ehkä antaa vinkin
    /// </summary>
    private void LuoIntroKuva()
    {
        // Luodaan kuva-objekti, joka kattaa koko tason
        PhysicsObject tarinaKuva = new PhysicsObject(Level.Width, Level.Height);
        tarinaKuva.Position = Level.Center;
        tarinaKuva.Image = LoadImage("tarinaKuva");
        tarinaKuva.Tag = "IntroKuva";
        tarinaKuva.CanRotate = false;
        Add(tarinaKuva);

        // Lisätään kuuntelijat WASD-näppäimille, jotka poistavat kuvan
        Keyboard.Listen(Key.W, ButtonState.Pressed, () => PoistaKuva(tarinaKuva), "Poista tarinakuva");
        Keyboard.Listen(Key.A, ButtonState.Pressed, () => PoistaKuva(tarinaKuva), "Poista tarinakuva");
        Keyboard.Listen(Key.S, ButtonState.Pressed, () => PoistaKuva(tarinaKuva), "Poista tarinakuva");
        Keyboard.Listen(Key.D, ButtonState.Pressed, () => PoistaKuva(tarinaKuva), "Poista tarinakuva");
    }


    /// <summary>
    /// Poistaa kuvan ja aloittaa ajastimen
    /// </summary>
    /// <param name="kuva">Alkuteksti kuva "intro"</param>
    private void PoistaKuva(GameObject kuva)
    {
        peliAjastin.Start();
        kuva.Destroy();
    }


    /// <summary>
    /// Luodaan peliin ajastin
    /// </summary>
    /// <param name="intervalli">Haluttu toiminto 5sec välein</param>
    /// <param name="toiminto">kello</param>
    private void LuoAjastin(double intervalli, Action toiminto)
    {
        Timer ajastin = new Timer();
        ajastin.Interval = intervalli;
        ajastin.Timeout += toiminto;
        ajastin.Start();
        if (aika.Value <= 0) ajastin.Stop();
    }


    /// <summary>
    /// Luo uuden kellon ajastimen avulla.
    /// </summary>
    private void LuoKello()
    {
        // Rajojen sisäpuolelle jäävä alue
        double minX = Level.Left + 40; // Esineen leveys
        double maxX = Level.Right - 40;
        double minY = Level.Bottom + 40; // Esineen korkeus
        double maxY = Level.Top - 40;

        // Arvotaan satunnainen sijainti rajojen sisällä
        double randomX = RandomGen.NextDouble(minX, maxX);
        double randomY = RandomGen.NextDouble(minY, maxY);

        // Luo esine
        int elinika = 10000;
        PhysicsObject kello = new PhysicsObject(30, 30); // Esineen koko (leveys, korkeus)
        kello.Position = new Vector(randomX, randomY); // Sijoita satunnaisesti
        kello.Color = Color.Yellow; // Esineen väri
        kello.Tag = "kello"; // Asetetaan tag esineelle, joka voidaan tunnistaa
        kello.CanRotate = false; // Estetään esineen pyöriminen
        kello.LifetimeLeft = TimeSpan.FromMilliseconds(elinika);

        kello.Image = LoadImage("clock");
        kello.IgnoresCollisionResponse = true;

        Add(kello);
    }

    
    /// <summary>
    /// Luodaan näyttö ja laitetaan se esille mikäli ensimmäinen tehtävä on suoritettu 
    /// </summary>
    /// <param name="t1">tehtävä1</param>
    private void LuoNaytto(bool t1)
    {
        if (t1) // Tarkistetaan, onko tehtävä 1 suoritettu
        {
            PhysicsObject naytto = new PhysicsObject(60, 60);
            naytto.Position = new Vector(Level.Right - 100, Level.Top - 270);
            naytto.Image = LoadImage("KoodiNaytto");
            naytto.CanRotate = false;
            naytto.IgnoresCollisionResponse = true;

            Add(naytto);
        }
    }


    /// <summary>
    /// Luodaan pelaaja hahmo
    /// </summary>
    /// <param name="peli">Pelattava peli</param>
    /// <param name="x">Pelaajan koko</param>
    /// <param name="y">Pelaajan koko</param>
    /// <returns>Palauttaa hahmon peliin</returns>
    private PhysicsObject LuoPelaaja(double x, double y)
    {
        // Luo pelaaja-objekti
        PhysicsObject pelaaja = new PhysicsObject(100.0, 100.0, Shape.Rectangle)
        {
            X = x,
            Y = y,
            CanRotate = false,
            IgnoresCollisionResponse = true
        };

        // Lataa animaation framet
        Image[] animaatioKuvat =
        {
            LoadImage("hahmoLiike1"),
            LoadImage("hahmoLiike2"),
        };

        // Luo animaatio
        Animation animaatio = new Animation(animaatioKuvat)
        {
            FPS = 10 // Aseta animaation nopeus (framea sekunnissa)
        };

        // Lisätään animaatio pelaajalle
        pelaaja.Animation = animaatio;

        // Pelaaja ei ole vielä liikkumassa, animaatio on pysäytetty
        pelaaja.Animation.Stop();

        // Lisätään pelaaja peliin
        Add(pelaaja);

        // Seurataan pelaajan liikettä
        Timer liikuntaTarkistus = new Timer();
        liikuntaTarkistus.Interval = 0.1; // Tarkistetaan liikkumista useammin, esim. 0.1 sekunnin välein
        liikuntaTarkistus.Timeout += () =>
        {
            if (Keyboard.IsKeyDown(Key.D) || Keyboard.IsKeyDown(Key.A) || Keyboard.IsKeyDown(Key.W) ||
                Keyboard.IsKeyDown(Key.S))
            {
                if (!pelaaja.Animation.IsPlaying) // Jos animaatio ei ole jo käynnissä
                {
                    pelaaja.Animation.Start(); // Käynnistä animaatio
                }
            }
            else
            {
                if (pelaaja.Animation.IsPlaying) // Jos animaatio on käynnissä ja pelaaja ei liiku
                {
                    pelaaja.Animation.Stop(); // Pysäytä animaatio
                }
            }
        };
        liikuntaTarkistus.Start();

        return pelaaja;
    }


    /// <summary>
    /// Lisätään aikaa pelaajalle ja poistetaan kello pelistä
    /// </summary>
    /// <param name="hahmo">Pelattava hahmo</param>
    /// <param name="kello">Aikaa lisäävä esine</param>
    private void TormaysKelloon(PhysicsObject hahmo, PhysicsObject kello)
    {
        aika.Value += 15;
        keratty.Value++;
        Remove(kello);
    }


    /// <summary>
    /// Laskuri joka kertoo montako kelloa olet kerännyt TODO: Myöhemmin myös scoreboard
    /// </summary>
    /// <returns>palauttaa laskurin</returns>
    private IntMeter KeratytKellot()
    {
        keratty = new IntMeter(0); // Luodaan laskuri
        Label keratytlabel = new Label();
        keratytlabel.BindTo(keratty); // Sidotaan laskuri labeliin
        keratytlabel.TextColor = Color.Red; // Määritetään väri
        keratytlabel.Position = new Vector(Screen.Right - 100, Screen.Top - 40); // Sijoitetaan label
        Add(keratytlabel); // Lisätään label peliin
        return keratty; // Palautetaan laskuri
    }



    /// <summary>
    /// Ohjelma tarkastelee aikaa ja ajaa lopetus mikäli aika loppuu
    /// </summary>
    private void VahennaAikaa()
    {
        aika.Value--;
        if (aika.Value <= 0)
        {
            peliAjastin.Stop();
            LuoAjastin(5, LuoKello);
            Havisit();
        }
    }


    /// <summary>
    /// Luodaan pelaajalle kyky liikkua
    /// </summary>
    /// <param name="pelaaja">pelattava hahmo</param>
    private void AsetaOhjaimet(PhysicsObject pelaaja)
    {
        Vector[] suunnat =
        {
            new(0, 200), // Ylös (W)
            new(0, -200), // Alas (S)
            new(-200, 0), // Vasemmalle (A)
            new(200, 0) // Oikealle (D)
        };

        Key[] nappaimet = { Key.W, Key.S, Key.A, Key.D };

        for (int i = 0; i < nappaimet.Length; i++)
        {
            Keyboard.Listen(nappaimet[i], ButtonState.Pressed, AsetaNopeus, null, pelaaja, suunnat[i]);
            Keyboard.Listen(nappaimet[i], ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
        }

        Keyboard.Listen(Key.H, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Enter, ButtonState.Pressed, TarkistaVastaus, "Tarkista pelaajan vastaus");
    }



    /// <summary>
    /// Luodaan pelaajalle nopeus
    /// </summary>
    /// <param name="pelaaja">Pelattava hahmo</param>
    /// <param name="nopeus">Annettaan pelaajalle nopeus</param>
    private static void AsetaNopeus(PhysicsObject pelaaja, Vector nopeus)
    {
        pelaaja.Velocity = nopeus;
    }


    /// <summary>
    /// Luodaan rajatut seinät 
    /// </summary>
    private void LuoRajatutSeinat()
    {
        Level.CreateLeftBorder(0, false);
        Level.CreateBottomBorder(0, false);
        Level.CreateRightBorder(0, false);
        Level.CreateTopBorder(0, false);

        _pelaaja.IgnoresCollisionResponse = false;

        //var a = Level.CreateTopBorder(0, true);
        //a.Height = 100;
        // Säädetään korkeus ja leveys
    }


    /// <summary>
    /// Häviämiseen liittyvä "fade to black" efekti
    /// </summary>
    private void Havisit()
    {
        // Asetetaan kuva koko näytölle
        PhysicsObject loppu = new PhysicsObject(Level.Width, Level.Height);
        loppu.X = Level.Center.X;
        loppu.Y = Level.Center.Y;
        loppu.Image = LoadImage("the_end");
        Add(loppu);

        // Voit lisätä myös muita toimintoja, kuten peliin äänen toiston
        // Game.PlaySound("KoiranAani1");

    }


    /// <summary>
    /// Luo keskelle ruutua yläosaan tekstin, jota voi käyttää pelin eri vaiheissa.
    /// </summary>
    /// <param name="teksti">Näytettävä teksti</param>
    /// <param name="koko">Tekstin koko</param>
    /// <returns>Palauttaa luodun Label-olion</returns>
    public Label Teksti(string teksti, double koko = 40)
    {
        Label label = new Label(teksti)
        {
            Position = new Vector(0, Screen.Top - 70), // Keskellä ruudun yläosaa

        };
        label.Color = Color.Red;

        Add(label); // Lisätään label peliin
        return label; // Palautetaan label, jotta sen ominaisuuksia voi muokata myöhemmin
    }

    
    /// <summary>
    /// Näyttää viestin ja poistaa sen 5 sekunnin kuluttua.
    /// </summary>
    /// <param name="teksti">Näytettävä viesti</param>
    private void NäytäViestilläAjastin(string teksti)
    {
        MessageDisplay.Add(teksti);

        // Luo ajastin, joka tyhjentää viestin 5 sekunnin jälkeen
        Timer ajastin = new Timer();
        ajastin.Interval = 5.0; // Sekuntia
        ajastin.Timeout += TyhjennäViestit;
        ajastin.Start();
    }

    /// <summary>
    /// Tyhjentää kaikki viestit MessageDisplaystä.
    /// </summary>
    private void TyhjennäViestit()
    {
        MessageDisplay.Clear();
    }

    
    /// <summary>
    /// Tarkastellaan aikaa jokaisella framella
    /// </summary>
    /// <param name="time">aika</param>
    protected override void Update(Time time)
    {
        base.Update(time);
        puzzle1(_pelaaja);
    }


    /// <summary>
    /// Ensimmäinen puzzle1. Pelaajan pitää kerätä 5 kelloa ja odottaa että aika laskee alle 30
    /// </summary>
    /// <param name="hahmo">pelattava hahmo</param>
    // ReSharper disable once MemberCanBePrivate.Global
    public void puzzle1(PhysicsObject hahmo)
    {
        
        if (aika.Value <= 30 && keratty.Value >= 5)
        {
            Teksti("Something has changed. I should check that old clock");
            task1 = true;
        }
        else
        {
            task1 = false;
        }
    }
    
    
    
    void LuoKysymys()
    {
        MessageDisplay.Add("Guess the 4-digit code to open the lock!");
    }

    
    
    void LisääNumero(string numero)
    {
        pelaajanVastaus += numero; // Lisää syötetty numero vastaukseen
    }
    
    
    
    void PoistaNumero()
    {
        if (pelaajanVastaus.Length > 0)
        {
            pelaajanVastaus = pelaajanVastaus.Remove(pelaajanVastaus.Length - 1);
        }
    }


    
    void TarkistaVastaus()
    {
        if (pelaajanVastaus == "1715")
        {
            MessageDisplay.Add("Door Opened");
        }
        else
        {
            MessageDisplay.Add("*peep sound_effect. Much spoopy*");
        }
    }
    
    
    /// <summary>
    /// Luo toisen puzzle-esineen, kuten lukon tai oven.
    /// </summary>
    /// <param name="x">Lukon sijainti X-akselilla</param>
    /// <param name="y">Lukon sijainti Y-akselilla</param>
    /// <param name="t1">Onko tehtävä 1 suoritettu</param>
    private void LuoPuzzle2Lukko(double x, double y, bool t1)
    {
        if (t1)
        {
        // Luo lukko-objekti
        PhysicsObject lukko = new PhysicsObject(100, 200, Shape.Rectangle);
        lukko.Position = new Vector(x, y);
        lukko.Image = LoadImage("lukko");
        lukko.CanRotate = false;
        lukko.IgnoresCollisionResponse = true;
        lukko.Tag = "lukko";

        Add(lukko);

        // Lisää tekstinäyttö ohjeeksi pelaajalle
        Label ohje = new Label("Solve the code to open the lock!");
        ohje.Position = new Vector(lukko.X, lukko.Y + 100);
        Add(ohje);

        // Kuuntele vastausta lukon avaamiseksi
        Keyboard.Listen(Key.Enter, ButtonState.Pressed, () => TarkistaPuzzle2Vastaus(lukko, ohje),
            "Tarkista pelaajan vastaus lukolle");
        }   
    }

    /// <summary>
    /// Tarkistaa, onko pelaajan syöttämä vastaus oikea ja poistaa lukon.
    /// </summary>
    /// <param name="lukko">Lukko-objekti, joka poistetaan oikealla vastauksella</param>
    /// <param name="ohje">Ohjeen näyttö, joka poistetaan lukon kanssa</param>
    private void TarkistaPuzzle2Vastaus(PhysicsObject lukko, Label ohje)
    {
        if (pelaajanVastaus == "1715")
        {
            MessageDisplay.Add("The lock opens with a click!");
            Remove(lukko);
            Remove(ohje);

            // Voit lisätä tähän seuraavan vaiheen logiikan, kuten oven avaamisen
            PhysicsObject ovi = new PhysicsObject(100, 150, Shape.Rectangle)
            {
                Position = new Vector(lukko.X, lukko.Y - 200),
                Color = Color.Green,
                IgnoresCollisionResponse = true
            };
            Add(ovi);
            MessageDisplay.Add("A door appears. Escape quickly!");
        }
        else
        {
            MessageDisplay.Add("The lock buzzes. Wrong code!");
        }
    }


    
    /// <summary>
    /// Tehtävä 2 jossa pitää laittaa oikeat luvut jotta peli etenee
    /// </summary>
    /// <param name="hahmo">pelattava hahmo</param>
    /// <param name="t1">tarkastellaan aiempaa tehtävää</param>
    public void puzzle2(PhysicsObject hahmo, bool t1)
    {
        if (t1)
        {
            LuoPuzzle2Lukko(Level.Center.X, Level.Center.Y, t1);
            LuoKysymys();

            // Kuuntele numeron lisäystä ja poistamista
            Keyboard.Listen(Key.Enter, ButtonState.Pressed, TarkistaVastaus, "Tarkista pelaajan vastaus");
            Keyboard.Listen(Key.Back, ButtonState.Pressed, PoistaNumero, "Poista viimeinen numero");
            for (int i = 0; i <= 9; i++)
            {
                int numero = i; // Lukitse paikallinen muuttuja delegaatille
                Keyboard.Listen((Key)Enum.Parse(typeof(Key), "D" + numero), ButtonState.Pressed, LisääNumero,
                    "Lisää numero " + numero, numero.ToString());
            }
        }
    }
    
    /// <summary>
    /// Luo lukko-objektin peliin ja lisää törmäyksen käsittelijän.
    /// </summary>
    /// <param name="x">Lukon sijainti X-akselilla</param>
    /// <param name="y">Lukon sijainti Y-akselilla</param>
    private void LuoLukkoJaTormays(double x, double y)
    {
        // Luo lukko-objekti
        PhysicsObject lukko = new PhysicsObject(150, 250, Shape.Rectangle);
        lukko.Position = new Vector(Level.Right -60, Level.Top - 350);
        lukko.Tag = "lukko";
        lukko.IsVisible = true;
        lukko.CanRotate = false;
        Add(lukko);

        // Lisää törmäyksen käsittelijä pelaajan ja lukon välille
        AddCollisionHandler(_pelaaja, lukko, AloitaNumeronArvaus);
    }
    

    /// <summary>
    /// Käynnistää numeron arvaustehtävän, kun pelaaja osuu lukkoon.
    /// </summary>
    /// <param name="pelaaja">Pelaajan hahmo</param>
    /// <param name="lukko">Lukko-objekti</param>
    private void AloitaNumeronArvaus(PhysicsObject pelaaja, PhysicsObject lukko)
    {
        if (task1 == true)
        {
            MessageDisplay.Add("You found a lock! Guess the code to open it.");
            LuoKysymys(); // Luo kysymys ja aloita numeron arvaus

            // Kuuntele pelaajan syöttämät numerot ja vastaukset
            Keyboard.Listen(Key.E, ButtonState.Pressed, AloitaInput, "Aloita pelaajan vastaus");
        }
    }


    private void AloitaInput()
    {
        Keyboard.Clear();
        
        Keyboard.Listen(Key.D0, ButtonState.Pressed, NumeroInput, "0");
        Keyboard.Listen(Key.D1, ButtonState.Pressed, NumeroInput, "1");
        Keyboard.Listen(Key.D2, ButtonState.Pressed, NumeroInput, "2");
        Keyboard.Listen(Key.D3, ButtonState.Pressed, NumeroInput, "3");
        Keyboard.Listen(Key.D4, ButtonState.Pressed, NumeroInput, "4");
        Keyboard.Listen(Key.D5, ButtonState.Pressed, NumeroInput, "5");
        Keyboard.Listen(Key.D6, ButtonState.Pressed, NumeroInput, "6");
        Keyboard.Listen(Key.D7, ButtonState.Pressed, NumeroInput, "7");
        Keyboard.Listen(Key.D8, ButtonState.Pressed, NumeroInput, "8");
        Keyboard.Listen(Key.D9, ButtonState.Pressed, NumeroInput, "9");
        
        Keyboard.Listen(Key.Enter, ButtonState.Pressed, TarkistaVastaus, "Tarkista pelaajan vastaus");
        Keyboard.Listen(Key.Back, ButtonState.Pressed, PoistaNumero, "Poista viimeinen numero");
        for (int i = 0; i <= 9; i++)
        {
            int numero = i; // Lukitse paikallinen muuttuja delegaatille
            Keyboard.Listen((Key)Enum.Parse(typeof(Key), "D" + numero), ButtonState.Pressed, LisääNumero,
                "Lisää numero " + numero, numero.ToString());
        }
    }

    private void NumeroInput()
    {
        
    }

    public void exit(bool t1, bool t2)
    {
        if (t1 && t2)
        {
            Label oviAuki = new Label("Door has opened I must hurry in before timer runs out");
            oviAuki.Position = new Vector(0, 100);
            Add(oviAuki);
        }
    }
}
