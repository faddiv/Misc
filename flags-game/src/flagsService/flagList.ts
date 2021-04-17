export enum Continent {
  Africa,
  Asia,
  Europe,
  NorthAmerica,
  Oceania,
  SouthAmerica,
}

export interface FlagInfo {
  Pic: string;
  Eng: string;
  Hun: string;
  Loc?: Continent;
}

export const flags: FlagInfo[] = [
  { Pic: "001-ethiopia.svg", Eng: "Ethiopia", Hun: "Etiópia" },
  { Pic: "002-oman.svg", Eng: "Oman", Hun: "Omán" },
  { Pic: "003-tanzania.svg", Eng: "Tanzania", Hun: "Tanzánia" },
  { Pic: "004-slovenia.svg", Eng: "Slovenia", Hun: "Szlovénia" },
  { Pic: "006-mozambique.svg", Eng: "Mozambique", Hun: "Mozambik" },
  { Pic: "007-iraq.svg", Eng: "Iraq", Hun: "Irak" },
  { Pic: "008-lebanon.svg", Eng: "Lebanon", Hun: "Libanon", Loc: Continent.Asia },
  { Pic: "009-uganda.svg", Eng: "Uganda", Hun: "Uganda" },
  { Pic: "010-nigeria.svg", Eng: "Nigeria", Hun: "Nigéria" },
  { Pic: "011-italy.svg", Eng: "Italy", Hun: "Olaszország" },
  { Pic: "012-malta.svg", Eng: "Malta", Hun: "Málta" },
  { Pic: "013-tunisia.svg", Eng: "Tunisia", Hun: "Tunézia" },
  { Pic: "014-nicaragua.svg", Eng: "Nicaragua", Hun: "Nicaragua" },
  { Pic: "015-el-salvador.svg", Eng: "El Salvador", Hun: "El Salvador" },
  { Pic: "016-zambia.svg", Eng: "Zambia", Hun: "Zambia" },
  { Pic: "018-dominican-republic.svg", Eng: "Dominican Republic", Hun: "Dominikai Köztársaság" },
  { Pic: "019-qatar.svg", Eng: "Qatar", Hun: "Katar" },
  { Pic: "020-namibia.svg", Eng: "Namibia", Hun: "Namíbia" },
  { Pic: "021-mauritius.svg", Eng: "Mauritius", Hun: "Mauritius" },
  { Pic: "023-luxembourg.svg", Eng: "Luxembourg", Hun: "Luxemburg" },
  { Pic: "025-lithuania.svg", Eng: "Lithuania", Hun: "Litvánia" },
  { Pic: "026-jamaica.svg", Eng: "Jamaica", Hun: "Jamaica" },
  { Pic: "027-honduras.svg", Eng: "Honduras", Hun: "Honduras" },
  { Pic: "028-myanmar.svg", Eng: "Myanmar", Hun: "Mianmar" },
  { Pic: "029-kenya.svg", Eng: "Kenya", Hun: "Kenya" },
  { Pic: "030-cyprus.svg", Eng: "Cyprus", Hun: "Ciprus" },
  { Pic: "031-pakistan.svg", Eng: "Pakistan", Hun: "Pakisztán" },
  { Pic: "032-latvia.svg", Eng: "Latvia", Hun: "Lettország" },
  { Pic: "033-japan.svg", Eng: "Japan", Hun: "Japán" },
  { Pic: "034-kazakhstan.svg", Eng: "Kazakhstan", Hun: "Kazahsztán" },
  { Pic: "035-serbia.svg", Eng: "Serbia", Hun: "Szerbia" },
  { Pic: "037-north-korea.svg", Eng: "North Korea", Hun: "Észak-Korea" },
  { Pic: "038-uruguay.svg", Eng: "Uruguay", Hun: "Uruguay" },
  { Pic: "039-syria.svg", Eng: "Syria", Hun: "Szíria" },
  { Pic: "040-guatemala.svg", Eng: "Guatemala", Hun: "Guatemala" },
  { Pic: "041-iceland.svg", Eng: "Iceland", Hun: "Izland" },
  { Pic: "042-jordan.svg", Eng: "Jordan", Hun: "Jordánia" },
  { Pic: "043-monaco.svg", Eng: "Monaco", Hun: "Monaco" },
  { Pic: "044-spain.svg", Eng: "Spain", Hun: "Spanyolország" },
  { Pic: "045-slovakia.svg", Eng: "Slovakia", Hun: "Szlovákia" },
  { Pic: "047-panama.svg", Eng: "Panama", Hun: "Panama" },
  { Pic: "048-new-zealand.svg", Eng: "New Zealand", Hun: "Új Zéland" },
  { Pic: "049-ecuador.svg", Eng: "Ecuador", Hun: "Ecuador" },
  { Pic: "050-romania.svg", Eng: "Romania", Hun: "Románia" },
  { Pic: "051-chile.svg", Eng: "Chile", Hun: "Chile" },
  { Pic: "052-finland.svg", Eng: "Finland", Hun: "Finnország" },
  { Pic: "053-hungary.svg", Eng: "Hungary", Hun: "Magyarország" },
  { Pic: "054-belgium.svg", Eng: "Belgium", Hun: "Belgium" },
  { Pic: "055-south-korea.svg", Eng: "South Korea", Hun: "Dél-Korea" },
  { Pic: "056-malaysia.svg", Eng: "Malaysia", Hun: "Malayzia" },
  { Pic: "057-venezuela.svg", Eng: "Venezuela", Hun: "Venezuela" },
  { Pic: "058-norway.svg", Eng: "Norway", Hun: "Norvégia" },
  { Pic: "059-saudi-arabia.svg", Eng: "Saudi Arabia", Hun: "Szaud-Arábia" },
  { Pic: "060-israel.svg", Eng: "Israel", Hun: "Izrael", Loc: Continent.Asia },
  { Pic: "061-czech-republic.svg", Eng: "Czech Republic", Hun: "Cseh Köztársaság" },
  { Pic: "062-colombia.svg", Eng: "Colombia", Hun: "Colombia" },
  { Pic: "063-iran.svg", Eng: "Iran", Hun: "Irán" },
  { Pic: "064-argentina.svg", Eng: "Argentina", Hun: "Argentína" },
  { Pic: "065-ukraine.svg", Eng: "Ukraine", Hun: "Ukrajna" },
  { Pic: "066-germany.svg", Eng: "Germany", Hun: "Németország" },
  { Pic: "068-united-arab-emirates.svg", Eng: "United Arab Emirates", Hun: "Egyesült Arab Emírségek" },
  { Pic: "069-laos.svg", Eng: "Laos", Hun: "Laosz" },
  { Pic: "070-ireland.svg", Eng: "Ireland", Hun: "Írország" },
  { Pic: "071-greece.svg", Eng: "Greece", Hun: "Görögország" },
  { Pic: "072-denmark.svg", Eng: "Denmark", Hun: "Dánia" },
  { Pic: "073-sweden.svg", Eng: "Sweden", Hun: "Svédország" },
  { Pic: "074-peru.svg", Eng: "Peru", Hun: "Peru" },
  { Pic: "075-south-africa.svg", Eng: "South Africa", Hun: "Dél-afrikai Köztársaság" },
  { Pic: "076-philippines.svg", Eng: "Philippines", Hun: "Fülöp-szigetek" },
  { Pic: "077-france.svg", Eng: "France", Hun: "Franciaország" },
  { Pic: "078-indonesia.svg", Eng: "Indonesia", Hun: "Indonézia" },
  { Pic: "079-egypt.svg", Eng: "Egypt", Hun: "Egyiptom" },
  { Pic: "080-taiwan.svg", Eng: "Taiwan", Hun: "Tajvan" },
  { Pic: "086-morocco.svg", Eng: "Morocco", Hun: "Marokkó" },
  { Pic: "090-comoros.svg", Eng: "Comoros", Hun: "Comore-szigetek" },
  { Pic: "091-central-african-republic.svg", Eng: "Central African Republic", Hun: "Közép-Afrikai Köztársaság" },
  { Pic: "094-bhutan.svg", Eng: "Bhutan", Hun: "Bhután" },
  { Pic: "095-chad.svg", Eng: "Chad", Hun: "Csád" },
  { Pic: "096-cape-verde.svg", Eng: "Cape Verde", Hun: "Zöld-foki köztársaság" },
  { Pic: "097-switzerland.svg", Eng: "Switzerland", Hun: "Svájc" },
  { Pic: "098-benin.svg", Eng: "Benin", Hun: "Benin" },
  { Pic: "102-andorra.svg", Eng: "Andorra", Hun: "Andorra" },
  { Pic: "103-burundi.svg", Eng: "Burundi", Hun: "Burundi" },
  { Pic: "104-antigua-and-barbuda.svg", Eng: "Antigua and Barbuda", Hun: "Antigua és Barbuda" },
  { Pic: "106-cameroon.svg", Eng: "Cameroon", Hun: "Kamerun" },
  { Pic: "107-brunei.svg", Eng: "Brunei", Hun: "Brunei" },
  { Pic: "108-poland.svg", Eng: "Poland", Hun: "Lengyelország" },
  { Pic: "110-belarus.svg", Eng: "Belarus", Hun: "Fehéroroszország" },
  { Pic: "111-barbados.svg", Eng: "Barbados", Hun: "Barbados" },
  { Pic: "113-bosnia-and-herzegovina.svg", Eng: "Bosnia and Herzegovina", Hun: "Bosznia és Hercegovina" },
  { Pic: "115-belize.svg", Eng: "Belize", Hun: "Belize" },
  { Pic: "116-bahrain.svg", Eng: "Bahrain", Hun: "Bahrein" },
  { Pic: "117-albania.svg", Eng: "Albania", Hun: "Albánia" },
  { Pic: "118-burkina-faso.svg", Eng: "Burkina Faso", Hun: "Burkina Faso" },
  { Pic: "119-turkey.svg", Eng: "Turkey", Hun: "Törökország" },
  { Pic: "121-armenia.svg", Eng: "Armenia", Hun: "Örményország" },
  { Pic: "122-afghanistan.svg", Eng: "Afghanistan", Hun: "Afganisztán" },
  { Pic: "124-angola.svg", Eng: "Angola", Hun: "Angola" },
  { Pic: "125-azerbaijan.svg", Eng: "Azerbaijan", Hun: "Azerbajdzsán" },
  { Pic: "126-algeria.svg", Eng: "Algeria", Hun: "Algéria" },
  { Pic: "127-botswana.svg", Eng: "Botswana", Hun: "Botswana" },
  { Pic: "128-bangladesh.svg", Eng: "Bangladesh", Hun: "Banglades" },
  { Pic: "129-cuba.svg", Eng: "Cuba", Hun: "Kuba" },
  { Pic: "130-australia.svg", Eng: "Australia", Hun: "Ausztrália" },
  { Pic: "131-costa-rica.svg", Eng: "Costa Rica", Hun: "Costa Rica" },
  { Pic: "132-cambodia.svg", Eng: "Cambodia", Hun: "Kambodzsa" },
  { Pic: "133-bolivia.svg", Eng: "Bolivia", Hun: "Bolívia" },
  { Pic: "134-croatia.svg", Eng: "Croatia", Hun: "Horvátország" },
  { Pic: "135-bulgaria.svg", Eng: "Bulgaria", Hun: "Bulgária" },
  { Pic: "138-tonga.svg", Eng: "Tonga", Hun: "Tonga" },
  { Pic: "139-st-lucia.svg", Eng: "St Lucia", Hun: "Szent Lucia" },
  { Pic: "141-singapore.svg", Eng: "Singapore", Hun: "Szingapúr" },
  { Pic: "142-palau.svg", Eng: "Palau", Hun: "Palau" },
  { Pic: "145-fiji.svg", Eng: "Fiji", Hun: "Fidzsi-szigetek" },
  { Pic: "147-dominica.svg", Eng: "Dominica", Hun: "Dominika" },
  { Pic: "148-vanuatu.svg", Eng: "Vanuatu", Hun: "Vanuatu" },
  { Pic: "149-sierra-leone.svg", Eng: "Sierra Leone", Hun: "Sierra Leone" },
  { Pic: "150-seychelles.svg", Eng: "Seychelles", Hun: "Seychelle-szigetek" },
  { Pic: "151-kosovo.svg", Eng: "Kosovo", Hun: "Koszovó" },
  { Pic: "153-united-states-of-america.svg", Eng: "United States of America", Hun: "Amerikai egyesült államok" },
  { Pic: "154-guinea.svg", Eng: "Guinea", Hun: "Guinea" },
  { Pic: "159-gambia.svg", Eng: "Gambia", Hun: "Gambia" },
  { Pic: "160-st-vincent-and-the-grenadines.svg", Eng: "St Vincent and the Grenadines", Hun: "Szent Vincent és a Grenadine-szigetek" },
  { Pic: "161-south-sudan.svg", Eng: "South Sudan", Hun: "Dél Szudán" },
  { Pic: "162-somaliland.svg", Eng: "Somaliland", Hun: "Szomáliföld" },
  { Pic: "163-solomon-islands.svg", Eng: "Solomon Islands", Hun: "Salamon-szigetek" },
  { Pic: "164-vietnam.svg", Eng: "Vietnam", Hun: "Vietnám" },
  { Pic: "167-saint-kitts-and-nevis.svg", Eng: "Saint Kitts and Nevis", Hun: "Saint Kitts és Nevis" },
  { Pic: "170-palestine.svg", Eng: "Palestine", Hun: "Palesztina" },
  { Pic: "173-nauru.svg", Eng: "Nauru", Hun: "Nauru" },
  { Pic: "174-portugal.svg", Eng: "Portugal", Hun: "Portugália" },
  { Pic: "177-mauritania.svg", Eng: "Mauritania", Hun: "Mauritánia" },
  { Pic: "178-kuwait.svg", Eng: "Kuwait", Hun: "Kuvait" },
  { Pic: "182-grenada.svg", Eng: "Grenada", Hun: "Grenada" },
  { Pic: "184-thailand.svg", Eng: "Thailand", Hun: "Thaiföld" },
  { Pic: "186-gabon.svg", Eng: "Gabon", Hun: "Gabon" },
  { Pic: "188-virgin-islands.svg", Eng: "Virgin Islands", Hun: "Virgin-szigetek" },
  { Pic: "189-austria.svg", Eng: "Austria", Hun: "Ausztria" },
  { Pic: "190-vatican-city.svg", Eng: "Vatican City", Hun: "Vatikán város" },
  { Pic: "191-tuvalu.svg", Eng: "Tuvalu", Hun: "Tuvalu" },
  { Pic: "192-turkmenistan.svg", Eng: "Turkmenistan", Hun: "Türkmenisztán" },
  { Pic: "193-togo.svg", Eng: "Togo", Hun: "Togo" },
  { Pic: "194-bahamas.svg", Eng: "Bahamas", Hun: "Bahama-szigetek" },
  { Pic: "195-netherlands.svg", Eng: "Netherlands", Hun: "Hollandia" },
  { Pic: "197-suriname.svg", Eng: "Suriname", Hun: "Suriname" },
  { Pic: "198-somalia.svg", Eng: "Somalia", Hun: "Szomália" },
  { Pic: "201-sao-tome-and-principe.svg", Eng: "Sao Tome and Principe", Hun: "Sao Tome és Principe" },
  { Pic: "203-niger.svg", Eng: "Niger", Hun: "Niger" },
  { Pic: "204-micronesia.svg", Eng: "Micronesia", Hun: "Mikronézia" },
  { Pic: "205-marshall-island.svg", Eng: "Marshall Island", Hun: "Marshall-sziget" },
  { Pic: "206-canada.svg", Eng: "Canada", Hun: "Kanada" },
  { Pic: "207-mali.svg", Eng: "Mali", Hun: "Mali" },
  { Pic: "208-kyrgyzstan.svg", Eng: "Kyrgyzstan", Hun: "Kirgizisztán" },
  { Pic: "209-guinea-bissau.svg", Eng: "Guinea Bissau", Hun: "Bissau-Guinea" },
  { Pic: "210-eritrea.svg", Eng: "Eritrea", Hun: "Eritrea" },
  { Pic: "211-djibouti.svg", Eng: "Djibouti", Hun: "Dzsibuti" },
  { Pic: "214-san-marino.svg", Eng: "San Marino", Hun: "San Marino" },
  { Pic: "216-liechtenstein.svg", Eng: "Liechtenstein", Hun: "Liechtenstein" },
  { Pic: "217-india.svg", Eng: "India", Hun: "India" },
  { Pic: "218-liberia.svg", Eng: "Liberia", Hun: "Libéria" },
  { Pic: "219-yemen.svg", Eng: "Yemen", Hun: "Jemen" },
  { Pic: "220-uzbekistan.svg", Eng: "Uzbekistan", Hun: "Üzbégisztán" },
  { Pic: "221-sudan.svg", Eng: "Sudan", Hun: "Szudán" },
  { Pic: "222-sahrawi-arab-democratic-republic.svg", Eng: "Sahrawi Arab Democratic Republic", Hun: "Szaharai Arab Demokratikus Köztársaság" },
  { Pic: "223-republic-of-macedonia.svg", Eng: "Republic of Macedonia", Hun: "Macedónia Köztársaság" },
  { Pic: "225-libya.svg", Eng: "Libya", Hun: "Líbia" },
  { Pic: "226-east-timor.svg", Eng: "East Timor", Hun: "Kelet-Timor" },
  { Pic: "228-russia.svg", Eng: "Russia", Hun: "Oroszország" },
  { Pic: "229-papua-new-guinea.svg", Eng: "Papua New Guinea", Hun: "Pápua Új-Guinea" },
  { Pic: "230-montenegro.svg", Eng: "Montenegro", Hun: "Montenegró" },
  { Pic: "231-moldova.svg", Eng: "Moldova", Hun: "Moldova" },
  { Pic: "232-maldives.svg", Eng: "Maldives", Hun: "Maldív-szigetek" },
  { Pic: "235-trinidad-and-tobago.svg", Eng: "Trinidad and Tobago", Hun: "Trinidad és Tobago" },
  { Pic: "236-tajikistan.svg", Eng: "Tajikistan", Hun: "Tádzsikisztán" },
  { Pic: "238-sri-lanka.svg", Eng: "Sri Lanka", Hun: "Srí Lanka" },
  { Pic: "239-mexico.svg", Eng: "Mexico", Hun: "Mexikó" },
  { Pic: "240-republic-of-the-congo.svg", Eng: "Republic of the Congo", Hun: "Kongói Köztársaság" },
  { Pic: "241-equatorial-guinea.svg", Eng: "Equatorial Guinea", Hun: "Egyenlítői-Guinea" },
  { Pic: "242-zimbabwe.svg", Eng: "Zimbabwe", Hun: "Zimbabwe" },
  { Pic: "243-rwanda.svg", Eng: "Rwanda", Hun: "Ruanda" },
  { Pic: "244-lesotho.svg", Eng: "Lesotho", Hun: "Lesotho" },
  { Pic: "245-ivory-coast.svg", Eng: "Ivory Coast", Hun: "Elefántcsontpart" },
  { Pic: "247-haiti.svg", Eng: "Haiti", Hun: "Haiti" },
  { Pic: "248-samoa.svg", Eng: "Samoa", Hun: "Szamoa" },
  { Pic: "249-nepal.svg", Eng: "Nepal", Hun: "Nepál" },
  { Pic: "250-brazil.svg", Eng: "Brazil", Hun: "Brazília", Loc: Continent.NorthAmerica },
  { Pic: "251-mongolia.svg", Eng: "Mongolia", Hun: "Mongólia" },
  { Pic: "252-malawi.svg", Eng: "Malawi", Hun: "Malawi" },
  { Pic: "253-madagascar.svg", Eng: "Madagascar", Hun: "Madagaszkár" },
  { Pic: "255-ghana.svg", Eng: "Ghana", Hun: "Ghána" },
  { Pic: "256-georgia.svg", Eng: "Georgia", Hun: "Grúzia" },
  { Pic: "257-estonia.svg", Eng: "Estonia", Hun: "Észtország" },
  { Pic: "258-democratic-republic-of-congo.svg", Eng: "Democratic Republic of Congo", Hun: "Kongói Demokratikus Köztársaság" },
  { Pic: "259-senegal.svg", Eng: "Senegal", Hun: "Szenegál" },
  { Pic: "260-paraguay.svg", Eng: "Paraguay", Hun: "Paraguay" },
  { Pic: "261-china.svg", Eng: "China", Hun: "Kína" },
  { Pic: "262-united-kingdom.svg", Eng: "United Kingdom", Hun: "Egyesült Királyság" },
  { Pic: "263-eswatini.svg", Eng: "Eswatini", Hun: "Szváziföld" },
  { Pic: "264-guyana.svg", Eng: "Guyana", Hun: "Guyana" },
  { Pic: "265-kiribati.svg", Eng: "Kiribati", Hun: "Kiribati" },

];