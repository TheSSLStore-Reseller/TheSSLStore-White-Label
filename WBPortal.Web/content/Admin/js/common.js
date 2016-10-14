var adminImgPath = '/content/admin/images';
var password = false;
var email = false;
$(function () {
    $(".UpdateStatus").click(function () {
        var recordToUpdate = $(this).attr("data-id");
        if (recordToUpdate && recordToUpdate != '') {
            var objImg = $('#img-' + recordToUpdate);
            var imgsrc = objImg.attr("src").toString();

            objImg.attr({ src: adminImgPath + '/loading1.gif' });

            $.post("/admin/reseller/changestatus/" + recordToUpdate,
                       function (data) {
                           if (data.toString().toLowerCase() == "true") {
                               if (imgsrc.indexOf("deactive") > 0) {
                                   objImg.attr({ src: imgsrc.replace("deactive", "active") })
                                   objImg.attr({ title: "Active", alt: "Active" })
                                   if ($(".UpdateStatus").attr("data-type") && $(".UpdateStatus").attr("data-type") == "reseller")
                                       alert(objReselleractmail);
                               }
                               else {
                                   objImg.attr({ src: imgsrc.replace("active", "deactive") })
                                   objImg.attr({ title: "Deactivated", alt: "Deactivated" })
                               }
                           }
                       });
        }
    }
        );

    $(".Remove").click(function () {
        var recordToDelete = $(this).attr("data-id");
        if (recordToDelete && recordToDelete != '') {
            $.post("/admin/reseller/delete/" + recordToDelete,
                       function (data) {
                           if (data.toString().toLowerCase() == "true") {
                               //debugger;
                               $('#row-' + recordToDelete).fadeOut('slow');
                           }
                       });
        }
    }
               );
}
    );
$(function () {
    $('#txtEmail').blur(function () {
        var recordToCheck = $(this).attr("dataid");
        $('#pLoaderImg').html('<img alt="" src="' + adminImgPath + '/loading1.gif" border="0" />');
        $.post("/admin/reseller/CheckEmailExist/" + recordToCheck + "?email=" + escape($('#txtEmail').val()),
                function (data) {                    
                    if (data.toString().toLowerCase() == "true") {
                        email = true;
                        $('#pLoaderImg').html(objEmailExist);
                    }
                    else {
                        $('#pLoaderImg').html('');
                        email = false;
                    }
                    disableButton();
                }
            );
    }
        );


});
function ValidatePassword(objThis, btn, spMsg) {
    var regex = /(?=.*\d)(?=.*[a-zA-Z]).{8,}/;
    if (!regex.test(objThis.value)) {
        password = true;
        $('#' + spMsg).html(objmsg);
    }
    else {
        $('#' + spMsg).html('');
        password = false;
    }
    disableButton();
}

function disableButton() {
   
    if (password || email) {
        $('#btnSubmit').attr("disabled", true);
    }
    else {
        $('#btnSubmit').removeAttr("disabled");
    }
}