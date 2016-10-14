
$(function () {

    $("#VATNumber").blur(function () {

        var recordToCheck = $('#VATNumber').val();
        var countryID = $("#user_Address_CountryID").val();
        var NetAmount = (eval(OrderAmount) - eval(PromoDiscount));

        var IsVATValid = false;
        var countryName = $("#user_Address_CountryID option:selected").text();
        if (defaultVATCountryID == countryID) {
            IsVATValid = checkVATNumber(recordToCheck);
        }
        if (IsVATValid) {
            var url = CentarlUrlPath + "/checkout/payment/caltax?vatnumber=" + recordToCheck + "&CountryID=" + countryID + "&amount=" + parseFloat(NetAmount) + "&VatCountry=" + parseInt(defaultVATCountryID) + "&VATTax=" + parseInt(vatpercent);
            $('#pLoaderImg').html("Validating.....");
            // $.post(CentarlUrlPath + "/checkout/payment/calTax", { "vatnumber": recordToCheck, "CountryID": countryID, "amount":  parseFloat(NetAmount) },
            $.post(url,
                function (data) {
                    data = parseFloat(data);

                    if (data > 0) {
                        var payamount = parseFloat(PayableAmount); //parseFloat($("#PayableAmount").val());
                        var credit = AvailableCredit;

                        if (payamount > 0) {
                            payamount += data;
                            $("#PayableAmount").val(payamount);
                        }
                        else if (payamount = 0 && credit > 0) {
                            if (data > credit) {
                                payamount = data - credit;
                                $("#PayableAmount").val(payamount);
                            }
                        }

                        $("#Tax").val(data);

                        $("#vattaxamount").html($.format(data, "c"));
                        $('#trvattaxamount').show();
                        $("#payorderamount").html($.format(payamount, "c"));

                        $('#pLoaderImg').html('');
                        $('#trtax').show();
                    }
                    else {
                        $('#pLoaderImg').html('');
                        $('#trtax').hide();
                        $('#trvattaxamount').hide();
                    }
                });
        }
        else if (isvatapplicable) {

            countryvatcal();
        }
        else {

            $('#pLoaderImg').html("Invalid VAT Number");
            $('#VATNumber').val('');
            $("#payorderamount").html($.format(NetAmount, "c"));
            $("#vattaxamount").html($.format(0.0, "c"));
            $('#trvattaxamount').hide();
        }
    });

    $("#frmpayment").validate({
        errorElement: "em",
        success: function (label) {
            label.text("").addClass("success");
        },
        rules: {

            PayPalID: {
                // required: "#rbpp:checked",
                email: "#rbpp:checked"
            },
            MoneybookersID: {
                email: "#rbmb:checked"
            },
            CCNumber: {
                required: "#rbcc:checked",
                minlength: 14,
                maxlength: 16,
                number: true
            },
            CCName: { required: "#rbcc:checked" },
            Month: { required: "#rbcc:checked" },
            Year: { required: "#rbcc:checked" },
            CVV: { required: "#rbcc:checked", number: true, minlength: 3, maxlength: 4 }

        },
        messages: {
            CCName: { required: CCName },
            Month: { required: Month },
            Year: { required: Year },
            CVV: { required: CVV }
        }
    });

    $("#rbcc").click(function () {
        $('#CCcreditCARD').fadeIn('slow');
        $('#ppPaypal').fadeOut('slow');
        $('#ppMoneybookers').fadeOut('slow');
        //$("#rbcc").closest('label').attr('class').replace('deactbtn', 'actbtn');
        //$("#rbcc").parent('label').attr('class', 'actbtn');
        //$("#rbpp").parent('label').attr('class', 'deactbtn');
        //$("#rbmb").parent('label').attr('class', 'deactbtn');

    });


    $("#rbpp").click(function () {
        $('#ppPaypal').fadeIn('slow');
        $('#CCcreditCARD').fadeOut('slow');
        $('#ppMoneybookers').fadeOut('slow');
        $("#rbpp").parent('label').attr('class', 'actbtn');
        $("#rbcc").parent('label').attr('class', 'deactbtn');
        $("#rbmb").parent('label').attr('class', 'deactbtn');
    });

    $("#rbmb").click(function () {
        $('#ppMoneybookers').fadeIn('slow');
        $('#ppPaypal').fadeOut('slow');
        $('#CCcreditCARD').fadeOut('slow');
        $("#rbmb").parent('label').attr('class', 'actbtn');
        $("#rbpp").parent('label').attr('class', 'deactbtn');
        $("#rbcc").parent('label').attr('class', 'deactbtn');
    });

    $("#btnplaceOrder").click(function () {
        if (HidePlaceOrder()) {

            $('#tblPlaceOrder').fadeOut('slow');
            $('#tblShoppingCart').fadeOut('slow');
            $('#tblConfirm').fadeIn('slow');
            $('#ccnoCaption').html("");
        }
    });

    $("#btnback").click(function () {

        if (HidePlaceOrder()) {

            $('#tblPlaceOrder').fadeIn('slow');
            $('#tblConfirm').fadeOut('slow');
            $('#tblShoppingCart').fadeIn('slow');
        }
    });

    if (ISCC.toLowerCase() == 'true') {
        $("#rbcc").click();
        $("#rbcc").attr("checked", "checked");

    }
    else if (IsPayPal.toLowerCase() == 'true') {
        $("#rbpp").click();
        $("#rbpp").attr("checked", "checked");

    }
    else if (IsMoneybookers.toLowerCase() == 'true') {
        $("#rbmb").click();
        $("#rbmb").attr("checked", "checked");

    }


    //countryvatcal 


    $("#user_Address_CountryID").change(function () {
        countryvatcal();
    });
    countryvatcal();
});

function CheckValidation() {
    var isValid = $('#frmpayment').valid();
    if (isValid)
        document.getElementById('dvpayment').style.display = 'block';
    return isValid;
}

function countryvatcal() {
    //user_Address_CountryID
    $("#user_Address_CountryID").val();
    //var countryID = $("#user_Address_CountryID option:selected").val();
    var countryID = $("#user_Address_CountryID").val();
    if (countryID > 0 && defaultVATCountryID == countryID) {       
        var data = (vatpercent * PayableAmount) / 100;
        var payamount = parseFloat(PayableAmount); //parseFloat($("#PayableAmount").val());
        var credit = AvailableCredit;
        if (payamount > 0) {
            payamount += data;
            $("#PayableAmount").val(payamount);
        }
        else if (payamount = 0 && credit > 0) {
            if (data > credit) {
                payamount = data - credit;
                $("#PayableAmount").val(payamount);
            }
        }

        $("#Tax").val(data);
        $("#vattaxamount").html($.format(data, "c"));
        $('#trvattaxamount').show();
        $("#payorderamount").html($.format(payamount, "c"));

    } else {
        var NetAmount = (eval(OrderAmount) - eval(PromoDiscount));
        if ($('#VATNumber').val() != '')
            $('#pLoaderImg').html("Invalid VAT Number");

        $('#VATNumber').val('');
        $("#payorderamount").html($.format(NetAmount, "c"));
        $("#vattaxamount").html($.format(0.0, "c"));
        $('#trvattaxamount').hide();
    }




}

function HidePlaceOrder(btn) {
    if ($("#frmpayment").valid()) {

        var strBilling;
        var ddlCountry = $("#user_Address_CountryID option:selected").text();
        strBilling = $('#user_Address_Street').val() + ",<br/>" + $('#user_Address_City').val() + ", " + $('#user_Address_State').val() + ", " + $('#user_Address_Zip').val() + "<br/>" + ddlCountry + "<br/>Phone: " + $('#user_Address_Phone').val();
        $('#spBilling').html(strBilling);

        $("#tdorderamount").html($("#strorderamount").html());
        $("#tdcreditamount").html($("#strcredit").html());
        $("#tddiscount").html($("#strdiscount").html());
        $("#tdtax").html($("#vattaxamount").html());
        $("#tdpayableamount").html($("#payorderamount").html());

        if (parseFloat(PayableAmount) <= 0) {

            $('#tdpaymenttype').html("Credit Used");
            $('#CCtbody').hide();
        }
        else {

            if ($("#rbcc").is(":checked") && ISCC.toLowerCase() == 'true') {

                var cc = $('#CCNumber').val();
                if (cc.length > 4) {
                    cc = "XXXXXXXXXXXX" + cc.substring(cc.length - 4, cc.length)
                }

                $('#tdpaymenttype').html("Credit Card");
                $('#tdccname').html($('#CCName').val());
                $('#tdccnumber').html(cc);
                $('#tdcccvv').html($('#CVV').val());
                $('#tdccdate').html($("#Month option:selected").text() + "/" + $("#Year option:selected").text());
                //$('#tdcctype').html($("#CardType1 option:selected").text());
                $('#CCtbody').show();

            }
            else if ($("#rbpp").is(":checked") && IsPayPal.toLowerCase() == 'true') {
                $('#tdpaymenttype').html("PayPal"); //<br/>PaypalID: " + $('#PayPalID').val()
                $('#CCtbody').hide();

            }
            else if ($("#rbmb").is(":checked") && IsMoneybookers.toLowerCase() == 'true') {
                $('#tdpaymenttype').html("Moneybookers");
                //if ($('#MoneybookersID').val() != '')
                //   $('#tdpaymenttype').html("Moneybookers<br/>MoneybookerID: " + $('#MoneybookersID').val());
                $('#CCtbody').hide();

            }
        }

        window.scrollTo(0, 0);
        return true;
    }
    else {
        return false;
    }
}

