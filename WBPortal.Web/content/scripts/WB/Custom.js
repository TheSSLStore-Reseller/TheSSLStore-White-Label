var txtbox = false;

function redirectToCart(ddlprice, ddlqty) {


    var pid = ddlprice.options[ddlprice.selectedIndex].value;
    var qty = ddlqty.options[ddlqty.selectedIndex].value;

    window.location.href = "/shoppingcart/addtocart?ppid=" + pid + "&qty=" + qty;
}

function AddToCart(priceid, qty, san) {

    window.location.href = "/shoppingcart/addtocart?ppid=" + priceid + "&qty=" + qty + "&san=" + san;
}
function AddToCartwithserver(priceid, qty, san, server) {

    window.location.href = "/shoppingcart/addtocart?ppid=" + priceid + "&qty=" + qty + "&san=" + san + "&server=" + server;
}
function CheckRadioSelection() {
    var flg = false;
    for (i = 0; i < document.forms[0].elements.length; i++) {
        elm = document.forms[0].elements[i]
        if (elm.type == 'radio' && elm.name.indexOf('rbProdPricingList_') > -1) {
            if (elm.checked) {
                flg = true;

            }
        }
    }
    if (!flg)
        alert('Please select atleast one product.');
    return flg;
}

function SetValueInHiddenField(objvalue, objTarget) {
    if (objTarget != null) {
        objTarget.value = objvalue;

    }

}


function UnCheckOtherRadio(current) {
    current.checked = true;
    $('div.newpricetwonebox').addClass('newpriceonebox').removeClass('newpricetwonebox');
    $(current).parent().parent().addClass('newpricetwonebox').removeClass('newpriceonebox');

}

function Contactusform() {
    var txtCompany = document.getElementById('txtCompany').value;
    var txtName = document.getElementById('txtFullname').value;
    var txtEmail = document.getElementById('txtEmail').value;
    var txtComment = document.getElementById('txtComment').value;
    var txtPhone = document.getElementById('txtPhone').value;


    if (txtCompany && txtCompany != '' && txtComment && txtComment != '' && txtPhone && txtPhone != '') {
        // Perform the ajax post
        $.post("/staticpage/contactus", { "CompanyName": txtCompany, "Name": txtName, "Phone": txtPhone, "Email": txtEmail, "Comment": txtComment },
                    function (data) {
                        // Successful requests get here
                        // Update the page elements                        
                        if (data) {
                            alert('Mail send sucessfully..!');
                            txtComment = "";
                            txtName = "";
                            txtEmail = "";
                            txtPhone = "";
                            txtCompany = "";
                        }
                        else {

                        }
                    });
    }

}



$(function () {
    //hdfpricingid
    //hdfqtyid
    //ddlSAN

    $("#btnaddtocartsan").click(function () {       
        var san = $.find("#ddlSAN").length > 0 ? $("#ddlSAN").val() : 0;
        AddToCart($("#hdfpricingid").val(), $("#hdfqtyid").val(), san);
    });
    $("#btnaddtocart").click(function () {
        var san = $.find("#ddlSAN").length > 0 ? $("#ddlSAN").val() : 0;
        AddToCart($("#hdfpricingid").val(), $("#hdfqtyid").val(), san);
    });

    $("input[type='radio'].customradio").change(function () {
        if ($(this).is(":checked")) {
            $("#hdfpricingid").val($(this).val());
        }
    });
    $("input[type='radio'].customradio").each(function () {
        if ($(this).is(":checked")) {
            $("#hdfpricingid").val($(this).val());
        }
    });

    $(".customddlQnt").change(function () {
        $("#hdfqtyid").val($(this).val());
    });
    $(".customddlsan").change(function () {
        $("#ddlSAN").val($(this).val());
    });

    $(".customradio").change();
    $(".customddlQnt").change();
    if ($.find(".customddlsan").length > 0)
        $(".customddlsan").change();

});
