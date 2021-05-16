import { Continent } from "./continentList";

export interface FlagInfo {
  Pic: string;
  Eng: string;
  Hun: string;
  Loc: Continent|null;
}

export const flags: FlagInfo[] = [
  { Pic: "001-ethiopia.svg", Eng: "Ethiopia", Hun: "Etiópia", Loc: Continent.Africa },
  { Pic: "002-oman.svg", Eng: "Oman", Hun: "Omán", Loc: Continent.Asia },
  { Pic: "003-tanzania.svg", Eng: "Tanzania", Hun: "Tanzánia", Loc: Continent.Africa },
  { Pic: "004-slovenia.svg", Eng: "Slovenia", Hun: "Szlovénia", Loc: Continent.Europe },
  { Pic: "006-mozambique.svg", Eng: "Mozambique", Hun: "Mozambik", Loc: Continent.Africa },
  { Pic: "007-iraq.svg", Eng: "Iraq", Hun: "Irak", Loc: Continent.Asia },
  { Pic: "008-lebanon.svg", Eng: "Lebanon", Hun: "Libanon", Loc: Continent.Asia },
  { Pic: "009-uganda.svg", Eng: "Uganda", Hun: "Uganda", Loc: Continent.Africa },
  { Pic: "010-nigeria.svg", Eng: "Nigeria", Hun: "Nigéria", Loc: Continent.Africa },
  { Pic: "011-italy.svg", Eng: "Italy", Hun: "Olaszország", Loc: Continent.Europe },
  { Pic: "012-malta.svg", Eng: "Malta", Hun: "Málta", Loc: Continent.Europe },
  { Pic: "013-tunisia.svg", Eng: "Tunisia", Hun: "Tunézia", Loc: Continent.Africa },
  { Pic: "014-nicaragua.svg", Eng: "Nicaragua", Hun: "Nicaragua", Loc: Continent.NorthAmerica },
  { Pic: "015-el-salvador.svg", Eng: "El Salvador", Hun: "El Salvador", Loc: Continent.NorthAmerica },
  { Pic: "016-zambia.svg", Eng: "Zambia", Hun: "Zambia", Loc: Continent.Africa },
  { Pic: "018-dominican-republic.svg", Eng: "Dominican Republic", Hun: "Dominikai Köztársaság", Loc: Continent.NorthAmerica },
  { Pic: "019-qatar.svg", Eng: "Qatar", Hun: "Katar", Loc: Continent.Asia },
  { Pic: "020-namibia.svg", Eng: "Namibia", Hun: "Namíbia", Loc: Continent.Africa },
  { Pic: "021-mauritius.svg", Eng: "Mauritius", Hun: "Mauritius", Loc: Continent.Africa },
  { Pic: "023-luxembourg.svg", Eng: "Luxembourg", Hun: "Luxemburg", Loc: Continent.Europe },
  { Pic: "025-lithuania.svg", Eng: "Lithuania", Hun: "Litvánia", Loc: Continent.Europe },
  { Pic: "026-jamaica.svg", Eng: "Jamaica", Hun: "Jamaica", Loc: Continent.NorthAmerica },
  { Pic: "027-honduras.svg", Eng: "Honduras", Hun: "Honduras", Loc: Continent.NorthAmerica },
  { Pic: "028-myanmar.svg", Eng: "Myanmar", Hun: "Mianmar", Loc: Continent.Asia },
  { Pic: "029-kenya.svg", Eng: "Kenya", Hun: "Kenya", Loc: Continent.Africa },
  { Pic: "030-cyprus.svg", Eng: "Cyprus", Hun: "Ciprus", Loc: Continent.Asia },
  { Pic: "031-pakistan.svg", Eng: "Pakistan", Hun: "Pakisztán", Loc: Continent.Asia },
  { Pic: "032-latvia.svg", Eng: "Latvia", Hun: "Lettország", Loc: Continent.Europe },
  { Pic: "033-japan.svg", Eng: "Japan", Hun: "Japán", Loc: Continent.Asia },
  { Pic: "034-kazakhstan.svg", Eng: "Kazakhstan", Hun: "Kazahsztán", Loc: Continent.Asia },
  { Pic: "035-serbia.svg", Eng: "Serbia", Hun: "Szerbia", Loc: Continent.Europe },
  { Pic: "037-north-korea.svg", Eng: "North Korea", Hun: "Észak-Korea", Loc: Continent.Asia },
  { Pic: "038-uruguay.svg", Eng: "Uruguay", Hun: "Uruguay", Loc: Continent.SouthAmerica },
  { Pic: "039-syria.svg", Eng: "Syria", Hun: "Szíria", Loc: Continent.Asia },
  { Pic: "040-guatemala.svg", Eng: "Guatemala", Hun: "Guatemala", Loc: Continent.NorthAmerica },
  { Pic: "041-iceland.svg", Eng: "Iceland", Hun: "Izland", Loc: Continent.Europe },
  { Pic: "042-jordan.svg", Eng: "Jordan", Hun: "Jordánia", Loc: Continent.Asia },
  { Pic: "043-monaco.svg", Eng: "Monaco", Hun: "Monaco", Loc: Continent.Europe },
  { Pic: "044-spain.svg", Eng: "Spain", Hun: "Spanyolország", Loc: Continent.Europe },
  { Pic: "045-slovakia.svg", Eng: "Slovakia", Hun: "Szlovákia", Loc: Continent.Europe },
  { Pic: "047-panama.svg", Eng: "Panama", Hun: "Panama", Loc: Continent.NorthAmerica },
  { Pic: "048-new-zealand.svg", Eng: "New Zealand", Hun: "Új Zéland", Loc: Continent.Oceania },
  { Pic: "049-ecuador.svg", Eng: "Ecuador", Hun: "Ecuador", Loc: Continent.SouthAmerica },
  { Pic: "050-romania.svg", Eng: "Romania", Hun: "Románia", Loc: Continent.Europe },
  { Pic: "051-chile.svg", Eng: "Chile", Hun: "Chile", Loc: Continent.SouthAmerica },
  { Pic: "052-finland.svg", Eng: "Finland", Hun: "Finnország", Loc: Continent.Europe },
  { Pic: "053-hungary.svg", Eng: "Hungary", Hun: "Magyarország", Loc: Continent.Europe },
  { Pic: "054-belgium.svg", Eng: "Belgium", Hun: "Belgium", Loc: Continent.Europe },
  { Pic: "055-south-korea.svg", Eng: "South Korea", Hun: "Dél-Korea", Loc: Continent.Asia },
  { Pic: "056-malaysia.svg", Eng: "Malaysia", Hun: "Malajzia", Loc: Continent.Asia },
  { Pic: "057-venezuela.svg", Eng: "Venezuela", Hun: "Venezuela", Loc: Continent.SouthAmerica },
  { Pic: "058-norway.svg", Eng: "Norway", Hun: "Norvégia", Loc: Continent.Europe },
  { Pic: "059-saudi-arabia.svg", Eng: "Saudi Arabia", Hun: "Szaud-Arábia", Loc: Continent.Asia },
  { Pic: "060-israel.svg", Eng: "Israel", Hun: "Izrael", Loc: Continent.Asia },
  { Pic: "061-czech-republic.svg", Eng: "Czech Republic", Hun: "Cseh Köztársaság", Loc: Continent.Europe },
  { Pic: "062-colombia.svg", Eng: "Colombia", Hun: "Kolumbia", Loc: Continent.SouthAmerica },
  { Pic: "063-iran.svg", Eng: "Iran", Hun: "Irán", Loc: Continent.Asia },
  { Pic: "064-argentina.svg", Eng: "Argentina", Hun: "Argentína", Loc: Continent.SouthAmerica },
  { Pic: "065-ukraine.svg", Eng: "Ukraine", Hun: "Ukrajna", Loc: Continent.Europe },
  { Pic: "066-germany.svg", Eng: "Germany", Hun: "Németország", Loc: Continent.Europe },
  { Pic: "068-united-arab-emirates.svg", Eng: "United Arab Emirates", Hun: "Egyesült Arab Emírségek", Loc: Continent.Asia },
  { Pic: "069-laos.svg", Eng: "Laos", Hun: "Laosz", Loc: Continent.Asia },
  { Pic: "070-ireland.svg", Eng: "Ireland", Hun: "Írország", Loc: Continent.Europe },
  { Pic: "071-greece.svg", Eng: "Greece", Hun: "Görögország", Loc: Continent.Europe },
  { Pic: "072-denmark.svg", Eng: "Denmark", Hun: "Dánia", Loc: Continent.Europe },
  { Pic: "073-sweden.svg", Eng: "Sweden", Hun: "Svédország", Loc: Continent.Europe },
  { Pic: "074-peru.svg", Eng: "Peru", Hun: "Peru", Loc: Continent.SouthAmerica },
  { Pic: "075-south-africa.svg", Eng: "South Africa", Hun: "Dél-afrikai Köztársaság", Loc: Continent.Africa },
  { Pic: "076-philippines.svg", Eng: "Philippines", Hun: "Fülöp-szigetek", Loc: Continent.Asia },
  { Pic: "077-france.svg", Eng: "France", Hun: "Franciaország", Loc: Continent.Europe },
  { Pic: "078-indonesia.svg", Eng: "Indonesia", Hun: "Indonézia", Loc: Continent.Asia },
  { Pic: "079-egypt.svg", Eng: "Egypt", Hun: "Egyiptom", Loc: Continent.Africa },
  //{ Pic: "080-taiwan.svg", Eng: "Taiwan", Hun: "Tajvan", Loc: Continent.Asia },//Missing
  { Pic: "086-morocco.svg", Eng: "Morocco", Hun: "Marokkó", Loc: Continent.Africa },
  { Pic: "090-comoros.svg", Eng: "Comoros", Hun: "Comore-szigetek", Loc: Continent.Africa },
  { Pic: "091-central-african-republic.svg", Eng: "Central African Republic", Hun: "Közép-Afrikai Köztársaság", Loc: Continent.Africa },
  { Pic: "094-bhutan.svg", Eng: "Bhutan", Hun: "Bhután", Loc: Continent.Asia },
  { Pic: "095-chad.svg", Eng: "Chad", Hun: "Csád", Loc: Continent.Africa },
  { Pic: "096-cape-verde.svg", Eng: "Cape Verde", Hun: "Zöld-foki köztársaság", Loc: Continent.Africa },
  { Pic: "097-switzerland.svg", Eng: "Switzerland", Hun: "Svájc", Loc: Continent.Europe },
  { Pic: "098-benin.svg", Eng: "Benin", Hun: "Benin", Loc: Continent.Africa },
  { Pic: "102-andorra.svg", Eng: "Andorra", Hun: "Andorra", Loc: Continent.Europe },
  { Pic: "103-burundi.svg", Eng: "Burundi", Hun: "Burundi", Loc: Continent.Africa },
  { Pic: "104-antigua-and-barbuda.svg", Eng: "Antigua and Barbuda", Hun: "Antigua és Barbuda", Loc: Continent.NorthAmerica },
  { Pic: "106-cameroon.svg", Eng: "Cameroon", Hun: "Kamerun", Loc: Continent.Africa },
  { Pic: "107-brunei.svg", Eng: "Brunei", Hun: "Brunei", Loc: Continent.Asia },
  { Pic: "108-poland.svg", Eng: "Poland", Hun: "Lengyelország", Loc: Continent.Europe },
  { Pic: "110-belarus.svg", Eng: "Belarus", Hun: "Fehéroroszország", Loc: Continent.Europe },
  { Pic: "111-barbados.svg", Eng: "Barbados", Hun: "Barbados", Loc: Continent.NorthAmerica },
  { Pic: "113-bosnia-and-herzegovina.svg", Eng: "Bosnia and Herzegovina", Hun: "Bosznia és Hercegovina", Loc: Continent.Europe },
  { Pic: "115-belize.svg", Eng: "Belize", Hun: "Belize", Loc: Continent.NorthAmerica },
  { Pic: "116-bahrain.svg", Eng: "Bahrain", Hun: "Bahrein", Loc: Continent.Asia },
  { Pic: "117-albania.svg", Eng: "Albania", Hun: "Albánia", Loc: Continent.Europe },
  { Pic: "118-burkina-faso.svg", Eng: "Burkina Faso", Hun: "Burkina Faso", Loc: Continent.Africa },
  { Pic: "119-turkey.svg", Eng: "Turkey", Hun: "Törökország", Loc: Continent.Asia },
  { Pic: "121-armenia.svg", Eng: "Armenia", Hun: "Örményország", Loc: Continent.Asia },
  { Pic: "122-afghanistan.svg", Eng: "Afghanistan", Hun: "Afganisztán", Loc: Continent.Asia },
  { Pic: "124-angola.svg", Eng: "Angola", Hun: "Angola", Loc: Continent.Africa },
  { Pic: "125-azerbaijan.svg", Eng: "Azerbaijan", Hun: "Azerbajdzsán", Loc: Continent.Asia },
  { Pic: "126-algeria.svg", Eng: "Algeria", Hun: "Algéria", Loc: Continent.Africa },
  { Pic: "127-botswana.svg", Eng: "Botswana", Hun: "Botswana", Loc: Continent.Africa },
  { Pic: "128-bangladesh.svg", Eng: "Bangladesh", Hun: "Banglades", Loc: Continent.Asia },
  { Pic: "129-cuba.svg", Eng: "Cuba", Hun: "Kuba", Loc: Continent.NorthAmerica },
  { Pic: "130-australia.svg", Eng: "Australia", Hun: "Ausztrália", Loc: Continent.Oceania },
  { Pic: "131-costa-rica.svg", Eng: "Costa Rica", Hun: "Costa Rica", Loc: Continent.NorthAmerica },
  { Pic: "132-cambodia.svg", Eng: "Cambodia", Hun: "Kambodzsa", Loc: Continent.Asia },
  { Pic: "133-bolivia.svg", Eng: "Bolivia", Hun: "Bolívia", Loc: Continent.SouthAmerica },
  { Pic: "134-croatia.svg", Eng: "Croatia", Hun: "Horvátország", Loc: Continent.Europe },
  { Pic: "135-bulgaria.svg", Eng: "Bulgaria", Hun: "Bulgária", Loc: Continent.Europe },
  { Pic: "138-tonga.svg", Eng: "Tonga", Hun: "Tonga", Loc: Continent.Oceania },
  { Pic: "139-st-lucia.svg", Eng: "Saint Lucia", Hun: "Szent Lucia", Loc: Continent.NorthAmerica },
  { Pic: "141-singapore.svg", Eng: "Singapore", Hun: "Szingapúr", Loc: Continent.Asia },
  { Pic: "142-palau.svg", Eng: "Palau", Hun: "Palau", Loc: Continent.Oceania },
  { Pic: "145-fiji.svg", Eng: "Fiji", Hun: "Fidzsi-szigetek", Loc: Continent.Oceania },
  { Pic: "147-dominica.svg", Eng: "Dominica", Hun: "Dominikai közösség", Loc: Continent.NorthAmerica },
  { Pic: "148-vanuatu.svg", Eng: "Vanuatu", Hun: "Vanuatu", Loc: Continent.Oceania },
  { Pic: "149-sierra-leone.svg", Eng: "Sierra Leone", Hun: "Sierra Leone", Loc: Continent.Africa },
  { Pic: "150-seychelles.svg", Eng: "Seychelles", Hun: "Seychelle-szigetek", Loc: Continent.Africa },
  { Pic: "151-kosovo.svg", Eng: "Kosovo", Hun: "Koszovó", Loc: Continent.Europe },
  { Pic: "153-united-states-of-america.svg", Eng: "United States of America", Hun: "Amerikai egyesült államok", Loc: Continent.NorthAmerica },
  { Pic: "154-guinea.svg", Eng: "Guinea", Hun: "Guinea", Loc: Continent.Africa },
  { Pic: "159-gambia.svg", Eng: "Gambia", Hun: "Gambia", Loc: Continent.Africa },
  { Pic: "160-st-vincent-and-the-grenadines.svg", Eng: "St Vincent and the Grenadines", Hun: "Szent Vincent és a Grenadine-szigetek", Loc: Continent.NorthAmerica },
  { Pic: "161-south-sudan.svg", Eng: "South Sudan", Hun: "Dél Szudán", Loc: Continent.Africa },
  //{ Pic: "162-somaliland.svg", Eng: "Somaliland", Hun: "Szomáliföld", Loc: Continent.Africa }, // Missing
  { Pic: "163-solomon-islands.svg", Eng: "Solomon Islands", Hun: "Salamon-szigetek", Loc: Continent.Oceania },
  { Pic: "164-vietnam.svg", Eng: "Vietnam", Hun: "Vietnám", Loc: Continent.Asia },
  { Pic: "167-saint-kitts-and-nevis.svg", Eng: "Saint Kitts and Nevis", Hun: "Saint Kitts és Nevis", Loc: Continent.NorthAmerica },
  //{ Pic: "170-palestine.svg", Eng: "Palestine", Hun: "Palesztina", Loc: Continent.Asia }, // Missing
  { Pic: "173-nauru.svg", Eng: "Nauru", Hun: "Nauru", Loc: Continent.Oceania },
  { Pic: "174-portugal.svg", Eng: "Portugal", Hun: "Portugália", Loc: Continent.Europe },
  { Pic: "177-mauritania.svg", Eng: "Mauritania", Hun: "Mauritánia", Loc: Continent.Africa },
  { Pic: "178-kuwait.svg", Eng: "Kuwait", Hun: "Kuvait", Loc: Continent.Asia },
  { Pic: "182-grenada.svg", Eng: "Grenada", Hun: "Grenada", Loc: Continent.NorthAmerica },
  { Pic: "184-thailand.svg", Eng: "Thailand", Hun: "Thaiföld", Loc: Continent.Asia },
  { Pic: "186-gabon.svg", Eng: "Gabon", Hun: "Gabon", Loc: Continent.Africa },
  { Pic: "189-austria.svg", Eng: "Austria", Hun: "Ausztria", Loc: Continent.Europe },
  { Pic: "190-vatican-city.svg", Eng: "Vatican City", Hun: "Vatikán város", Loc: Continent.Europe },
  { Pic: "191-tuvalu.svg", Eng: "Tuvalu", Hun: "Tuvalu", Loc: Continent.Oceania },
  { Pic: "192-turkmenistan.svg", Eng: "Turkmenistan", Hun: "Türkmenisztán", Loc: Continent.Asia },
  { Pic: "193-togo.svg", Eng: "Togo", Hun: "Togo", Loc: Continent.Africa },
  { Pic: "194-bahamas.svg", Eng: "Bahamas", Hun: "Bahama-szigetek", Loc: Continent.NorthAmerica },
  { Pic: "195-netherlands.svg", Eng: "Netherlands", Hun: "Hollandia", Loc: Continent.Europe },
  { Pic: "197-suriname.svg", Eng: "Suriname", Hun: "Suriname", Loc: Continent.SouthAmerica },
  { Pic: "198-somalia.svg", Eng: "Somalia", Hun: "Szomália", Loc: Continent.Africa },
  { Pic: "201-sao-tome-and-principe.svg", Eng: "Sao Tome and Principe", Hun: "Sao Tome és Principe", Loc: Continent.Africa },
  { Pic: "203-niger.svg", Eng: "Niger", Hun: "Niger", Loc: Continent.Africa },
  { Pic: "204-micronesia.svg", Eng: "Micronesia", Hun: "Mikronézia", Loc: Continent.Oceania },
  { Pic: "205-marshall-island.svg", Eng: "Marshall Island", Hun: "Marshall-sziget", Loc: Continent.Oceania },
  { Pic: "206-canada.svg", Eng: "Canada", Hun: "Kanada", Loc: Continent.NorthAmerica },
  { Pic: "207-mali.svg", Eng: "Mali", Hun: "Mali", Loc: Continent.Africa },
  { Pic: "208-kyrgyzstan.svg", Eng: "Kyrgyzstan", Hun: "Kirgizisztán", Loc: Continent.Asia },
  { Pic: "209-guinea-bissau.svg", Eng: "Guinea Bissau", Hun: "Bissau-Guinea", Loc: Continent.Africa },
  { Pic: "210-eritrea.svg", Eng: "Eritrea", Hun: "Eritrea", Loc: Continent.Africa },
  { Pic: "211-djibouti.svg", Eng: "Djibouti", Hun: "Dzsibuti", Loc: Continent.Africa },
  { Pic: "214-san-marino.svg", Eng: "San Marino", Hun: "San Marino", Loc: Continent.Europe },
  { Pic: "216-liechtenstein.svg", Eng: "Liechtenstein", Hun: "Liechtenstein", Loc: Continent.Europe },
  { Pic: "217-india.svg", Eng: "India", Hun: "India", Loc: Continent.Asia },
  { Pic: "218-liberia.svg", Eng: "Liberia", Hun: "Libéria", Loc: Continent.Africa },
  { Pic: "219-yemen.svg", Eng: "Yemen", Hun: "Jemen", Loc: Continent.Asia },
  { Pic: "220-uzbekistan.svg", Eng: "Uzbekistan", Hun: "Üzbégisztán", Loc: Continent.Asia },
  { Pic: "221-sudan.svg", Eng: "Sudan", Hun: "Szudán", Loc: Continent.Africa },
  //{ Pic: "222-sahrawi-arab-democratic-republic.svg", Eng: "Sahrawi Arab Democratic Republic", Hun: "Szaharai Arab Demokratikus Köztársaság", Loc: Continent.Africa }, // Missing
  { Pic: "223-republic-of-macedonia.svg", Eng: "Republic of Macedonia", Hun: "Macedónia Köztársaság", Loc: Continent.Europe },
  { Pic: "225-libya.svg", Eng: "Libya", Hun: "Líbia", Loc: Continent.Africa },
  { Pic: "226-east-timor.svg", Eng: "East Timor", Hun: "Kelet-Timor", Loc: Continent.Asia },
  { Pic: "228-russia.svg", Eng: "Russia", Hun: "Oroszország", Loc: Continent.Asia },
  { Pic: "229-papua-new-guinea.svg", Eng: "Papua New Guinea", Hun: "Pápua Új-Guinea", Loc: Continent.Oceania },
  { Pic: "230-montenegro.svg", Eng: "Montenegro", Hun: "Montenegró", Loc: Continent.Europe },
  { Pic: "231-moldova.svg", Eng: "Moldova", Hun: "Moldova", Loc: Continent.Europe },
  { Pic: "232-maldives.svg", Eng: "Maldives", Hun: "Maldív-szigetek", Loc: Continent.Asia },
  { Pic: "235-trinidad-and-tobago.svg", Eng: "Trinidad and Tobago", Hun: "Trinidad és Tobago", Loc: Continent.NorthAmerica },
  { Pic: "236-tajikistan.svg", Eng: "Tajikistan", Hun: "Tádzsikisztán", Loc: Continent.Asia },
  { Pic: "238-sri-lanka.svg", Eng: "Sri Lanka", Hun: "Srí Lanka", Loc: Continent.Asia },
  { Pic: "239-mexico.svg", Eng: "Mexico", Hun: "Mexikó", Loc: Continent.NorthAmerica },
  { Pic: "240-republic-of-the-congo.svg", Eng: "Republic of the Congo", Hun: "Kongói Köztársaság", Loc: Continent.Africa },
  { Pic: "241-equatorial-guinea.svg", Eng: "Equatorial Guinea", Hun: "Egyenlítői-Guinea", Loc: Continent.Africa },
  { Pic: "242-zimbabwe.svg", Eng: "Zimbabwe", Hun: "Zimbabwe", Loc: Continent.Africa },
  { Pic: "243-rwanda.svg", Eng: "Rwanda", Hun: "Ruanda", Loc: Continent.Africa },
  { Pic: "244-lesotho.svg", Eng: "Lesotho", Hun: "Lesotho", Loc: Continent.Africa },
  { Pic: "245-ivory-coast.svg", Eng: "Ivory Coast", Hun: "Elefántcsontpart", Loc: Continent.Africa },
  { Pic: "247-haiti.svg", Eng: "Haiti", Hun: "Haiti", Loc: Continent.NorthAmerica },
  { Pic: "248-samoa.svg", Eng: "Samoa", Hun: "Szamoa", Loc: Continent.Oceania },
  { Pic: "249-nepal.svg", Eng: "Nepal", Hun: "Nepál", Loc: Continent.Asia },
  { Pic: "250-brazil.svg", Eng: "Brazil", Hun: "Brazília", Loc: Continent.NorthAmerica },
  { Pic: "251-mongolia.svg", Eng: "Mongolia", Hun: "Mongólia", Loc: Continent.Asia },
  { Pic: "252-malawi.svg", Eng: "Malawi", Hun: "Malawi", Loc: Continent.Africa },
  { Pic: "253-madagascar.svg", Eng: "Madagascar", Hun: "Madagaszkár", Loc: Continent.Africa },
  { Pic: "255-ghana.svg", Eng: "Ghana", Hun: "Ghána", Loc: Continent.Africa },
  { Pic: "256-georgia.svg", Eng: "Georgia", Hun: "Grúzia", Loc: Continent.Asia },
  { Pic: "257-estonia.svg", Eng: "Estonia", Hun: "Észtország", Loc: Continent.Europe },
  { Pic: "258-democratic-republic-of-congo.svg", Eng: "Democratic Republic of Congo", Hun: "Kongói Demokratikus Köztársaság", Loc: Continent.Africa },
  { Pic: "259-senegal.svg", Eng: "Senegal", Hun: "Szenegál", Loc: Continent.Africa },
  { Pic: "260-paraguay.svg", Eng: "Paraguay", Hun: "Paraguay", Loc: Continent.SouthAmerica },
  { Pic: "261-china.svg", Eng: "China", Hun: "Kína", Loc: Continent.Asia },
  { Pic: "262-united-kingdom.svg", Eng: "United Kingdom", Hun: "Egyesült Királyság", Loc: Continent.Europe },
  { Pic: "263-eswatini.svg", Eng: "Eswatini", Hun: "Szváziföld", Loc: Continent.Africa },
  { Pic: "264-guyana.svg", Eng: "Guyana", Hun: "Guyana", Loc: Continent.SouthAmerica },
  { Pic: "265-kiribati.svg", Eng: "Kiribati", Hun: "Kiribati", Loc: Continent.Oceania },
];
