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
            var s = $(this).is(':checked') ? true : false;
            $.post("/admin/testimonials/changestatus/" + recordToUpdate + "?s=" + (imgsrc.indexOf("deactive") > 0 ? "1" : "0"),
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

                                   $('#IchkTestimonials-' + recordToUpdate).attr('checked', false);
                                  
                               }
                           }
                       });
        }
    }
        );

    $(function () {
        $("input[name='IchkTestimonials']").click(function () {
            var recordToUpdate = $(this).attr("data-id");
            if (recordToUpdate && recordToUpdate != '') {
                var objImg = $('#img-' + recordToUpdate);
                var imgsrc = objImg.attr("src").toString();
                
                var s = $(this).is(':checked') ? true : false;
                
                objImg.attr({ src: adminImgPath + '/loading1.gif' });
             
                $.post("/admin/testimonials/changestatuspage/" + recordToUpdate + "?s="+s,

                       function (data) {
                           
                           if (data.toString().toLowerCase() == "true") {
                               if (imgsrc.indexOf("deactive") > 0) {

                                   objImg.attr({ src: imgsrc.replace("deactive", "active") })

                                   objImg.attr({ title: "Active", alt: "Active" })
                                   if ($("input[name='IchkTestimonials']").attr("data-type") && $("input[name='IchkTestimonials']").attr("data-type") == "reseller")
                                       alert(objReselleractmail);
                               }
                               else {
                                   objImg.attr({ src: imgsrc.replace("deactive", "active") })
                                   objImg.attr({ title: "Active", alt: "Active" })
                               }
                           }
                       });
            }
        }
        );
    });

    $(".Remove").click(function () {
        var recordToDelete = $(this).attr("data-id");
        if (recordToDelete && recordToDelete != '') {
            $.post("/admin/testimonials/delete/" + recordToDelete,
                       function (data) {
                           if (data.toString().toLowerCase() == "true") {

                               $('#row-' + recordToDelete).fadeOut('slow');
                           }
                       });
        }
    }
               );
}
    );

$(function () {
    $('#ddlStatus').change(function () { StatusRow(); });
});

function f() {
    if (statusid) {
        var status = statusid;
        var showpage = showinpage;
        if (status == 3) {
            $('#ddlStatus').val(1);
        }
        StatusRow();
    }
}

function StatusRow() {
    var ddlvalue = $("#ddlStatus").val();
    if (ddlvalue == '0') {
        $('#statusrow').hide();
        $('#chkTestimonials').attr('checked', false);
    }
    else {
        $('#statusrow').show();
    }
}
