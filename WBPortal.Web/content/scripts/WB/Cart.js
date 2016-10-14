var path = (window.location.host.toString().indexOf('centralapi') > -1 ? CentarlUrlPath : '');
//centralapi
$(function () {
    // Document.ready -> link up remove event handler    
    $(".RemoveLink").click(function () {
        if (confirm("Are you sure you want to delete this product ?")) {
            // Get the id from the link
             
            var recordToDelete = $(this).attr("data-id");

            if (recordToDelete != '') {
                
                // Perform the ajax post
                $.post(path + "/shoppingcart/removefromcart", { "id": recordToDelete },
                    function (data) {
                        // Successful requests get here
                        // Update the page elements

                        if (parseInt(data.ItemCount) > 0) {                                                
                            $('#row-' + data.Id).remove();
                            $('#sanrow-' + data.Id).remove();
                            $('#prow-' + data.Id).remove();

                            $('#cart-total').text(data.CartTotal);
                            alert(data.Message);
                            if (window.location.host.toString().indexOf('centralapi') > -1)
                                window.location.reload();
                        }
                        else {
                            window.location.href = "/";
                        }

                    });
            }
        }
    });

});

function ApplyPromo(msg) {
    var pcode = document.getElementById("textpromo").value;

    if (pcode && pcode != '') {
        // Perform the ajax post
        $.post(path + "/shoppingcart/applypromo", { "code": pcode },
                    function (data) {
                        // Successful requests get here
                        // Update the page elements                        
                        if (data && parseInt(data.ItemCount) > 0) {
                            var Ids = data.Id.toString().split("|-----|");
                            var promos = data.promodiscount.toString().split("|-----|");
                            var iCnt = 0;
                            for (s in Ids) {
                                if (Ids[s] != '') {
                                    $('#prow-' + Ids[s]).fadeIn('slow');
                                    $('#promodiscount-' + Ids[s]).text(promos[iCnt]);
                                    iCnt = iCnt + 1;
                                }
                            }
                            $('#cart-total').text(data.CartTotal);
                            alert(data.Message);
                        }
                        else {
                            alert(data.Message);
                        }
                    });

    }
    else {
        alert(msg);
    }
}

function EditSuccess() {
    window.location.href = path + "/shoppingcart";
}