using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    public static List<Card> cardList = new List<Card>();

    Color32 color_black = new Color32(0, 0, 0, 255);
        Color32 color_white = new Color32(255, 255, 255, 255);
        Color32 card2 = new Color32(249, 254, 128, 255);
        Color32 card3 = new Color32(246, 175, 25, 255);
        Color32 card4 = new Color32(242, 253, 255, 255);
        Color32 card5 = new Color32(217, 224, 206, 255);
        Color32 card6 = new Color32(221, 221, 197, 255);
        Color32 card7 = new Color32(202, 231, 25, 255);
        Color32 card8 = new Color32(244, 184, 2, 255);
        Color32 card9 = new Color32(252, 61, 42, 255);
        Color32 card10 = new Color32(253, 251, 138, 255);
        Color32 card11 = new Color32(222, 205, 161, 255);
        Color32 card12 = new Color32(252, 248, 247, 255);
        Color32 card15 = new Color32(214, 203, 173, 255);
        Color32 card16 = new Color32(248, 231, 143, 255);
        Color32 card18 = new Color32(161, 156, 152, 255);
        Color32 card19 = new Color32(255, 255, 160, 255);
        Color32 card20 = new Color32(179, 223, 234, 255);
        Color32 card21 = new Color32(238, 11, 30, 255);
        Color32 card22 = new Color32(254, 233, 166, 255);
        Color32 card25 = new Color32(226, 229, 234, 255);
        Color32 card26 = new Color32(241, 214, 111, 255);
        Color32 card27 = new Color32(15, 185, 24, 255); 
        Color32 card28 = new Color32(74, 5, 8, 255);
        Color32 card29 = new Color32(236, 3, 0, 255); 
        Color32 card31 = new Color32(118, 122, 97, 255); 
        Color32 card32 = new Color32(248, 254, 250, 255);
        Color32 card33 = new Color32(255, 38, 83, 255); 
        Color32 card34 = new Color32(202, 197, 93, 255);
        Color32 card35 = new Color32(234, 211, 19, 255); 
        Color32 card36 = new Color32(247, 212, 110, 255);
        Color32 card37 = new Color32(132, 131, 127, 255);
        Color32 card38 = new Color32(207, 164, 129, 255);
        Color32 card39 = new Color32(197, 1, 11, 255);
        Color32 card40 = new Color32(156, 194, 153, 255);
        Color32 card41 = new Color32(255, 181, 24, 255);
        Color32 card42 = new Color32(113, 87, 114, 255); 
        Color32 card43 = new Color32(250, 239, 207, 255);
        Color32 card44 = new Color32(3, 15, 113, 255);
        Color32 card45 = new Color32(133, 25, 57, 255);
        Color32 card46 = new Color32(255, 227, 84, 255);
        Color32 card48 = new Color32(231, 249, 225, 255);
        Color32 card49 = new Color32(177, 196, 194, 255);
        Color32 card50 = new Color32(9, 94, 39, 255);
        Color32 card52 = new Color32(119, 125, 111, 255);
        Color32 card53 = new Color32(75, 143, 130, 255);
        Color32 card55 = new Color32(185, 0, 0, 255);
        Color32 card56 = new Color32(135, 133, 134, 255);
        Color32 card57 = new Color32(255, 89, 21, 255);
        Color32 card58 = new Color32(230, 229, 123, 255);
        Color32 card59 = new Color32(210, 54, 41, 255);
        Color32 card60 = new Color32(65, 66, 71, 255);
        Color32 card62 = new Color32(199, 213, 222, 255);
        Color32 card63 = new Color32(176, 207, 176, 255);
        Color32 card64 = new Color32(54, 253, 230, 255);
        Color32 card65 = new Color32(208, 213, 190, 255);
        Color32 card66 = new Color32(244, 244, 244, 255);
        Color32 card67 = new Color32(248, 153, 0, 255);
        Color32 card68 = new Color32(74, 44, 56, 255);
        Color32 card69 = new Color32(148, 4, 13, 255);
        Color32 card70 = new Color32(19, 6, 148, 255);
        Color32 card72 = new Color32(253, 235, 211, 255);
        Color32 card73 = new Color32(23, 24, 10, 255);
        Color32 card74 = new Color32(200, 2, 19, 255);
        Color32 card76 = new Color32(225, 198, 111, 255);
        Color32 card77 = new Color32(26, 203, 223, 255);
        Color32 card79 = new Color32(185, 15, 18, 255);
        Color32 card80 = new Color32(173, 127, 156, 255);
        Color32 card81 = new Color32(189, 197, 186, 255);
        Color32 card82 = new Color32(0, 169, 172, 255);
        Color32 card83 = new Color32(27, 217, 189, 255);
        Color32 card84 = new Color32(92, 118, 47, 255);
        Color32 card86 = new Color32(184, 34, 29, 255);
        Color32 card87 = new Color32(176, 6, 6, 255);
        Color32 card88 = new Color32(255, 189, 30, 255);
        Color32 card89 = new Color32(190, 6, 32, 255);
        Color32 card90 = new Color32(35, 38, 53, 255);
        Color32 card91 = new Color32(236, 28, 26, 255);
        Color32 card92 = new Color32(168, 223, 166, 255);
        Color32 card93 = new Color32(29, 26, 21, 255);
        Color32 card94 = new Color32(220, 144, 6, 255);
        Color32 card95 = new Color32(23, 10, 2, 255);
        Color32 card96 = new Color32(139, 17, 204, 255);
        Color32 card97 = new Color32(177, 1, 3, 255);
        Color32 card99 = new Color32(249, 250, 252, 255);



    void Awake()
    {

        

        cardList.Add(new Card(0,"",1,Resources.Load<Sprite>(""),color_black,2,10));
        cardList.Add(new Card(1,"Adolf Hilter",1,Resources.Load<Sprite>("hitler"),color_black,2,10));
        cardList.Add(new Card(2,"Jesus of Nazareth",1,Resources.Load<Sprite>("jezis"),card2,2,10));
        cardList.Add(new Card(3,"Henry Ford",1,Resources.Load<Sprite>("ford"),card3,2,10));
        cardList.Add(new Card(4,"Marie Curie",1,Resources.Load<Sprite>("curie"),card4,2,10));
        cardList.Add(new Card(5,"Abraham Lincoln",1,Resources.Load<Sprite>("lincoln"),card5,2,10));
        cardList.Add(new Card(6,"Vlad Tepes 'The Impaler'",1,Resources.Load<Sprite>("dracula"),card6,2,10));
        cardList.Add(new Card(7,"Marylin Monroe",1,Resources.Load<Sprite>("marylin"),card7,2,10));
        cardList.Add(new Card(8,"Saladin",1,Resources.Load<Sprite>("saladin"),card8,2,10));
        cardList.Add(new Card(9,"Steven Hawking",1,Resources.Load<Sprite>("hawkins"),card9,2,10));//preskoceny tayke utoky, bude nakoniec
        cardList.Add(new Card(10,"Sitting Bull",1,Resources.Load<Sprite>("sittingbull"),card10,2,10));
        cardList.Add(new Card(11,"Vladimir Iljic Lenin",1,Resources.Load<Sprite>("lenin"),card11,2,10));
        cardList.Add(new Card(12,"Nikola Tesla",1,Resources.Load<Sprite>("tesla"),card12,2,10));
        cardList.Add(new Card(13,"Lucky Luciano",1,Resources.Load<Sprite>("luciano"),color_black,2,10));
        cardList.Add(new Card(14,"The Wright Brothers",1,Resources.Load<Sprite>("wright"),color_black,2,10));//preskoceny len tak
        cardList.Add(new Card(15,"Eleanor Roosvelt",1,Resources.Load<Sprite>("eleanor"),card15,2,10));//preskoceny nuda
        cardList.Add(new Card(16,"Napoleon Bonaparte",1,Resources.Load<Sprite>("napoleon"),card16,2,10));
        cardList.Add(new Card(17,"Bruce Lee",1,Resources.Load<Sprite>("brucelee"),color_black,2,10));
        cardList.Add(new Card(18,"Vincent Van Gogh",1,Resources.Load<Sprite>("vangogh"),card18,2,10));
        cardList.Add(new Card(19,"Francis Drake",1,Resources.Load<Sprite>("drake"),card19,2,10));//preskoceny skareda karticka
        cardList.Add(new Card(20,"Harry Houdini",1,Resources.Load<Sprite>("houdini"),card20,2,10));
        cardList.Add(new Card(21,"Enzo Ferrari",1,Resources.Load<Sprite>("ferrari"),card21,2,10));
        cardList.Add(new Card(22,"Mata Hari",1,Resources.Load<Sprite>("matahari"),card22,2,10));
        cardList.Add(new Card(23,"Billy the Kid",1,Resources.Load<Sprite>("kid"),color_black,2,10));
        cardList.Add(new Card(24,"Wernher Von Braun",1,Resources.Load<Sprite>("vonbraun"),color_white,2,10));
        cardList.Add(new Card(25,"Joan of Arc",1,Resources.Load<Sprite>("johanka"),card25,2,10));
        cardList.Add(new Card(26,"William Shakespeare",1,Resources.Load<Sprite>("shakespeare"),card26,2,10));
        cardList.Add(new Card(27,"Bob Marley",1,Resources.Load<Sprite>("bob"),card27,2,10));
        cardList.Add(new Card(28,"Shaka Zulu",1,Resources.Load<Sprite>("shaka"),card28,2,10));
        cardList.Add(new Card(29,"Miyamoto Musashi",1,Resources.Load<Sprite>("musashi"),card29,2,10));
        cardList.Add(new Card(30,"Yasuke",1,Resources.Load<Sprite>("yasuke"),color_black,2,10));
        cardList.Add(new Card(31,"Boudica",1,Resources.Load<Sprite>("boudica"),card31,2,10));
        cardList.Add(new Card(32,"Carl Marx",1,Resources.Load<Sprite>("marx"),card32,2,10));
        cardList.Add(new Card(33,"Giacomo Casanova",1,Resources.Load<Sprite>("casanova"),card33,2,10));
        cardList.Add(new Card(34,"Confucius",1,Resources.Load<Sprite>("confucius"),card34,2,10));
        cardList.Add(new Card(35,"Mahatma Gandhi",1,Resources.Load<Sprite>("ghandi"),card35,2,10));
        cardList.Add(new Card(36,"Spatracus",1,Resources.Load<Sprite>("spartacus"),card36,2,10));
        cardList.Add(new Card(37,"Hatori Hanzo",1,Resources.Load<Sprite>("hanzo"),card37,2,10));
        cardList.Add(new Card(38,"Marco Polo",1,Resources.Load<Sprite>("polo"),card38,2,10));
        cardList.Add(new Card(39,"Ching Shih",1,Resources.Load<Sprite>("chingshih"),card39,2,10));
        cardList.Add(new Card(40,"Ragnar Lothbrok",1,Resources.Load<Sprite>("ragnar"),card40,2,10));
        cardList.Add(new Card(41,"Montezuma",1,Resources.Load<Sprite>("montezuma"),card41,2,10));
        cardList.Add(new Card(42,"Pablo Picasso",1,Resources.Load<Sprite>("picasso"),card42,2,10));
        cardList.Add(new Card(43,"Marlon Brando",1,Resources.Load<Sprite>("brando"),card43,2,10));
        cardList.Add(new Card(44,"Pele",1,Resources.Load<Sprite>("pele"),card44,2,10));
        cardList.Add(new Card(45,"La Voisin",1,Resources.Load<Sprite>("lavoisin"),card45,2,10));
        cardList.Add(new Card(46,"Siddhartha Gautama Buddha",1,Resources.Load<Sprite>("buddha"),card46,2,10));
        cardList.Add(new Card(47,"Henry VIII.",1,Resources.Load<Sprite>("henry"),color_black,2,10));
        cardList.Add(new Card(48,"Horatio Nelson",1,Resources.Load<Sprite>("nelson"),card48,2,10));
        cardList.Add(new Card(49,"Cleopatra",1,Resources.Load<Sprite>("cleopatra"),card49,2,10));
        cardList.Add(new Card(50,"Mansa Musa",1,Resources.Load<Sprite>("mansa"),card50,2,10));
        cardList.Add(new Card(51,"Pancho Villa",1,Resources.Load<Sprite>("pancho"),color_white,2,10));
        cardList.Add(new Card(52,"Aristotle",1,Resources.Load<Sprite>("aristotle"),card52,2,10));
        cardList.Add(new Card(53,"Alexander the great",1,Resources.Load<Sprite>("alexander"),card53,2,10));
        cardList.Add(new Card(54,"Martin Luther King",1,Resources.Load<Sprite>("king"),color_white,2,10));
        cardList.Add(new Card(55,"Che Guevara",1,Resources.Load<Sprite>("che"),card55,2,10));
        cardList.Add(new Card(56,"Pythagoras of samos",1,Resources.Load<Sprite>("pythagoras"),card56,2,10));
        cardList.Add(new Card(57,"Pocahontas",1,Resources.Load<Sprite>("pocahontas"),card57,2,10));
        cardList.Add(new Card(58,"Narmer",1,Resources.Load<Sprite>("narmer"),card58,2,10));
        cardList.Add(new Card(59,"Hammurabi",1,Resources.Load<Sprite>("hammurabi"),card59,2,10));
        cardList.Add(new Card(60,"King David I.",1,Resources.Load<Sprite>("david"),card60,2,10));
        cardList.Add(new Card(61,"Pope Leo I.",1,Resources.Load<Sprite>("leo"),color_black,2,10));
        cardList.Add(new Card(62,"Adam Smith",1,Resources.Load<Sprite>("smith"),card62,2,10));
        cardList.Add(new Card(63,"Manfred von Richthofen",1,Resources.Load<Sprite>("baron"),card63,2,10));
        cardList.Add(new Card(64,"Ada Lovelace",1,Resources.Load<Sprite>("lovelace"),card64,2,10));
        cardList.Add(new Card(65,"Mary Seacole",1,Resources.Load<Sprite>("seacole"),card65,2,10));
        cardList.Add(new Card(66,"Suzanne Lenglen",1,Resources.Load<Sprite>("lenglet"),card66,2,10));
        cardList.Add(new Card(67,"Sir Henry Morgan",1,Resources.Load<Sprite>("morgan"),card67,2,10));
        cardList.Add(new Card(68,"Percy Julian",1,Resources.Load<Sprite>("percy"),card68,2,10));
        cardList.Add(new Card(69,"Sun Tzu",1,Resources.Load<Sprite>("suntzu"),card69,2,10));
        cardList.Add(new Card(70,"White death",1,Resources.Load<Sprite>("whitedeath"),card70,2,10));
        cardList.Add(new Card(71,"Lyudmila Pavlichenko",1,Resources.Load<Sprite>("pavlichenko"),color_black,2,10));
        cardList.Add(new Card(72,"Jesse James",1,Resources.Load<Sprite>("jessejames"),card72,2,10));
        cardList.Add(new Card(73,"Leonardo Da Vinci",1,Resources.Load<Sprite>("leonardo"),card73,2,10));
        cardList.Add(new Card(74,"Chinggis Khan",1,Resources.Load<Sprite>("dzingischan"),card74,2,10));
        cardList.Add(new Card(75,"Milo of Croton",1,Resources.Load<Sprite>("milo"),color_black,2,10));
        cardList.Add(new Card(76,"Zopyrus",1,Resources.Load<Sprite>("zopyrus"),card76,2,10));
        cardList.Add(new Card(77,"Atotoztli II.",1,Resources.Load<Sprite>("atotoztli"),card77,2,10));
        cardList.Add(new Card(78,"Alexander Dubcek",1,Resources.Load<Sprite>("dubcek"),color_white,2,10));
        cardList.Add(new Card(79,"Wu Zetian",1,Resources.Load<Sprite>("wuzetian"),card79,2,10));
        cardList.Add(new Card(80,"Grigorij Jefimovic Rasputin",1,Resources.Load<Sprite>("rasputin"),card80,2,10));
        cardList.Add(new Card(81,"Moses",1,Resources.Load<Sprite>("moses"),card81,2,10));
        cardList.Add(new Card(82,"Black Elk",1,Resources.Load<Sprite>("blackelk"),card82,2,10));
        cardList.Add(new Card(83,"John Dee",1,Resources.Load<Sprite>("johndee"),card83,2,10));
        cardList.Add(new Card(84,"Diviciacus",1,Resources.Load<Sprite>("diviciacus"),card84,2,10));
        cardList.Add(new Card(85,"Bodhidharma",1,Resources.Load<Sprite>("bodhidharma"),color_black,2,10));
        cardList.Add(new Card(86,"James Figg",1,Resources.Load<Sprite>("figg"),card86,2,10));
        cardList.Add(new Card(87,"Tomoe Gozen",1,Resources.Load<Sprite>("tomoe"),card87,2,10));
        cardList.Add(new Card(88,"Robert Oppenheimer",1,Resources.Load<Sprite>("oppenheimer"),card88,2,10));
        cardList.Add(new Card(89,"Isaac Newton",1,Resources.Load<Sprite>("newton"),card89,2,10));
        cardList.Add(new Card(90,"Teuta",1,Resources.Load<Sprite>("teuta"),card90,2,10));
        cardList.Add(new Card(91,"Jurij Alexejevic Gagarin",1,Resources.Load<Sprite>("gagarin"),card91,2,10));
        cardList.Add(new Card(92,"Robin Hood",1,Resources.Load<Sprite>("hood"),card92,2,10));
        cardList.Add(new Card(93,"Auguste Escoffier",1,Resources.Load<Sprite>("escoffier"),card93,2,10));
        cardList.Add(new Card(94,"Imhotep",1,Resources.Load<Sprite>("imhotep"),card94,2,10));
        cardList.Add(new Card(95,"Wang Zongyue",1,Resources.Load<Sprite>("wang"),card95,2,10));
        cardList.Add(new Card(96,"Joseph Grimaldi",1,Resources.Load<Sprite>("grimaldi"),card96,2,10));
        cardList.Add(new Card(97,"Nzinga Mbande",1,Resources.Load<Sprite>("nzinga"),card97,2,10));
        cardList.Add(new Card(98,"WIld Bill Hickok",1,Resources.Load<Sprite>("wildbill"),color_black,2,10));
        cardList.Add(new Card(99,"Ibn al-Haytham",1,Resources.Load<Sprite>("ibn"),card99,2,10));




    }


}
