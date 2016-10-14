/*==============================================================================

// To change the default country (e.g. from the UK to Germany - DE):
//    1.  Change the country code in the defCCode variable below to "DE".
//    2.  Remove the question mark from the regular expressions associated 
//        with the UK VAT number: i.e. "(GB)?" -> "(GB)"
//    3.  Add a question mark into the regular expression associated with
//        Germany's number following the country code: i.e. "(DE)" -> "(DE)?"
                    
------------------------------------------------------------------------------*/

function checkVATNumber(toCheck) {
    // Array holds the regular expressions for the valid VAT number
  
    var vatexp = { AT: /^(AT)U(\d{8})$/,
        BE: /^(BE)0(\d{9})$/,
        BG: /^(BG)(\d{9,10})$/,                         // Bulgaria 
        CY: /^(CY)(\d{8}[A-Z])$/,                       //** Cyprus 
        CZ: /^(CZ)(\d{8,10})(\d{3})?$/,                 //** Czech Republic
        DE: /^(DE)(\d{9})$/,
        DK: /^(DK)((\d{8}))$/,
        EE: /^(EE)(\d{9})$/,
        EL: /^(EL)(\d{9})$/,
        ES: /^(ES)([A-Z]\d{8})$/,
        ES1:/^(ES)(\d{8}[A-Z])$/,
        ES2:/^(ES)([A-Z]\d{7}[A-Z])$/,
        FI:/^(FI)(\d{8})$/,
        FR:/^(FR)(\d{11})$/,
        FR1:/^(FR)[(A-H)|(J-N)|(P-Z)]\d{10}$/,
        FR2:/^(FR)\d[(A-H)|(J-N)|(P-Z)]\d{9}$/,
        FR3:/^(FR)[(A-H)|(J-N)|(P-Z)]{2}\d{9}$/,
        GB:/^(GB)?(\d{9})$/,
        GB1:/^(GB)?(\d{10})$/,
        GB2:/^(GB)?(\d{12})$/,
        GB3:/^(GB)?(\d{13})$/,
        GB4:/^(GB)?(GD\d{3})$/,
        GB5:/^(GB)?(HA\d{3})$/,
        GR:/^(GR)(\d{8,9})$/,
        HU:/^(HU)(\d{8})$/,
        IE:/^(IE)(\d{7}[A-W])$/,
        IE:/^(IE)([7-9][A-Z\*\+)]\d{5}[A-W])$/,
        IT:/^(IT)(\d{11})$/,
        LT:/^(LT)(\d{9}|\d{12})$/,
        LU:/^(LU)(\d{8})$/,
        LV:/^(LV)(\d{11})$/,
        MT:/^(MT)(\d{8})$/,
        NL:/^(NL)(\d{9})B\d{2}$/,
        PL:/^(PL)(\d{10})$/,
        PT:/^(PT)(\d{9})$/,
        RO:/^(RO)(\d{2,10})$/,
        SE:/^(SE)(\d{10}\d[1-4])$/,
        SI:/^(SI)(\d{8})$/,
        SK:/^(SK)(\d{9}|\d{10})$/
    };

    var defCCode = defaultVATCCode;  //"GB";

    // Load up the string to check
    var VATNumber = toCheck.toUpperCase();

    // Remove spaces from the VAT number to help validation
    var chars = [" ", "-", ",", "."];
    for (var i = 0; i < chars.length; i++) {
        while (VATNumber.indexOf(chars[i]) != -1) {
            VATNumber = VATNumber.slice(0, VATNumber.indexOf(chars[i])) + VATNumber.slice(VATNumber.indexOf(chars[i]) + 1);
        }
    }
    
    // Assume we're not going to find a valid VAT number
    var valid = false;
    // Check the string against the types of VAT numbers
    var charNumber = ["", "1", "2", "3", "4", "5"];
    for (var i = 0; i < charNumber.length; i++) {
        var countryregexp = eval( "vatexp." + defCCode + charNumber[i]);
        if (countryregexp) {
            if (countryregexp.test(VATNumber)) {

                var cCode = RegExp.$1;                             // Isolate country code
                var cNumber = RegExp.$2;                           // Isolate the number
                if (cCode.length == 0) cCode = defCCode;           // Set up default country code

                // Now look at the check digits for those countries we know about.
                switch (cCode) {
                    case "AT":
                        valid = ATVATCheckDigit(cNumber);
                        break;
                    case "BE":
                        valid = BEVATCheckDigit(cNumber);
                        break;
                    case "BG":
                        // The SIMA validation rules are incorrect for Bulgarian numbers.
                        //valid = BGVATCheckDigit (cNumber)
                        valid = true;
                        break;
                    case "CY":
                        valid = CYVATCheckDigit(cNumber);
                        break;
                    case "CZ":
                        valid = CZVATCheckDigit(cNumber);
                        break;
                    case "DE":
                        valid = DEVATCheckDigit(cNumber);
                        break;
                    case "DK":
                        valid = DKVATCheckDigit(cNumber);
                        break;
                    case "EE":
                        valid = EEVATCheckDigit(cNumber);
                        break;
                    case "EL":
                        valid = ELVATCheckDigit(cNumber);
                        break;
                    case "ES":
                        valid = ESVATCheckDigit(cNumber);
                        break;
                    case "EU":
                        valid = EUVATCheckDigit(cNumber);
                        break;
                    case "FI":
                        valid = FIVATCheckDigit(cNumber);
                        break;
                    case "FR":
                        valid = FRVATCheckDigit(cNumber);
                        break;
                    case "GB":
                        valid = UKVATCheckDigit(cNumber);
                        break;
                    case "GR":
                        valid = ELVATCheckDigit(cNumber);
                        break;
                    case "HU":
                        valid = HUVATCheckDigit(cNumber);
                        break;
                    case "IE":
                        valid = IEVATCheckDigit(cNumber);
                        break;
                    case "IT":
                        valid = ITVATCheckDigit(cNumber);
                        break;
                    case "LT":
                        valid = LTVATCheckDigit(cNumber);
                        break;
                    case "LU":
                        valid = LUVATCheckDigit(cNumber);
                        break;
                    case "LV":
                        valid = LVVATCheckDigit(cNumber);
                        break;
                    case "MT":
                        valid = MTVATCheckDigit(cNumber);
                        break;
                    case "NL":
                        valid = NLVATCheckDigit(cNumber);
                        break;
                    case "PL":
                        valid = PLVATCheckDigit(cNumber);
                        break;
                    case "PT":
                        valid = PTVATCheckDigit(cNumber);
                        break;
                    case "RO":
                        valid = ROVATCheckDigit(cNumber);
                        break;
                    case "SE":
                        valid = SEVATCheckDigit(cNumber);
                        break;
                    case "SI":
                        valid = SIVATCheckDigit(cNumber);
                        break;
                    default:
                        valid = true;
                }

                // Load new VAT number back into the form element
                if (valid) valid = VATNumber;

                // We have found that the number is valid - break from loop
                break;
            }
        }
    }

    // Return with either an error or the reformatted VAT number
    return valid;
}

function ATVATCheckDigit(vatnumber) {
    return true;
}

function BEVATCheckDigit(vatnumber) {
    return true;
}

function BGVATCheckDigit(vatnumber) {
    return true;
}

function CYVATCheckDigit(vatnumber) {
    return true;
}

function CZVATCheckDigit(vatnumber) {
    return true;
}

function DEVATCheckDigit(vatnumber) {
    return true;
}

function DKVATCheckDigit(vatnumber) {
    return true;
}

function EEVATCheckDigit(vatnumber) {
    return true;
}

function ELVATCheckDigit(vatnumber) {
    return true;
}

function ESVATCheckDigit(vatnumber) {
    return true;
}

function EUVATCheckDigit(vatnumber) {

    // We know litle about EU numbers apart from the fact that the first 3 digits 
    // represent the country, and that there are nine digits in total.
    return true;
}

function FIVATCheckDigit(vatnumber) {
    return true;
}

function FRVATCheckDigit(vatnumber) {
    return true;
}

function HUVATCheckDigit(vatnumber) {
    return true;
}

function IEVATCheckDigit(vatnumber) {
    return true;
}

function ITVATCheckDigit(vatnumber) {
    return true;
}

function LTVATCheckDigit(vatnumber) {
    return true;
}

function LUVATCheckDigit(vatnumber) {
    return true;
}

function LVVATCheckDigit(vatnumber) {
    return true;
}

function MTVATCheckDigit(vatnumber) {
    return true;
}

function NLVATCheckDigit(vatnumber) {
    return true;
}

function PLVATCheckDigit(vatnumber) {
    return true;
}

function PTVATCheckDigit(vatnumber) {
    return true;
}

function ROVATCheckDigit(vatnumber) {
    return true;
}

function SEVATCheckDigit(vatnumber) {
    return true;
}

function SKVATCheckDigit(vatnumber) {
    return true;
}

function SIVATCheckDigit(vatnumber) {

    return true;
}

function UKVATCheckDigit(vatnumber) {
    return true;
}
