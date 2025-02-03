$(document).ready(function () {

    function getGameIdFromUrl() {
        const urlParts = window.location.pathname.split('/');
        return parseInt(urlParts[urlParts.length - 1]); // Convert to integer to ensure it's a number
    }

    const gameId = getGameIdFromUrl();
    let selectedRating = 0;


    // Show modal when "Rate" link is clicked
    $('#rate-link').on('click', function () {

        // Fetch the user's existing rating for this game
        $.ajax({
            type: "GET",
            url: `/Product/GetUserRating/${gameId}`, // Adjust the URL according to your route
            success: function (response) {
                const userRating = response.rating;

                if (userRating) {
                    // Set the previously saved rating as the selected rating
                    selectedRating = userRating;
                    fillStarsUpTo(selectedRating);
                    $('#save-button').prop('disabled', false); // Enable button if a rating already exists
                }

                // Show the modal
                $('#rateModal').fadeIn();
            },
            error: function () {
                $('#rateModal').fadeIn(); // Show the modal even if rating retrieval fails
            }
        });
    });

    // Show modal when the single star is clicked
    $("#single-star").on("click", function () {
        $("#rateModal").fadeIn(); // Show modal with fade-in effect
    });

    // Close modal when the "X" button is clicked
    $(".close").on("click", function () {
        $("#rateModal").fadeOut(); // Hide modal with fade-out effect
    });

    // Close modal if user clicks outside of the modal content
    $(window).on("click", function (e) {
        if ($(e.target).is("#rateModal")) {
            $("#rateModal").fadeOut();
        }
    });

    // Handle star rating selection and hover
    $(".rating-star").on({
        mouseenter: function () {
            const starValue = $(this).data("value");
            fillStarsUpTo(starValue); // Temporarily fill up to hovered star
        },
        mouseleave: function () {
            fillStarsUpTo(selectedRating); // Revert to selected rating after hover
        },
        click: function () {
            selectedRating = $(this).data("value"); // Update selectedRating when user clicks
            fillStarsUpTo(selectedRating); // Fill stars up to the clicked rating
            $('#save-button').prop('disabled', false); // Enable button if a rating already exists
        },
    });

    // Function to fill stars up to a given rating without updating selectedRating
    function fillStarsUpTo(rating) {
        $(".rating-star").each(function () {
            const starValue = $(this).data("value");
            if (starValue <= rating) {
                $(this)
                    .removeClass("fa-regular")
                    .addClass("fa-solid")
                    .css("color", "#f5c518"); // Filled star color
            } else {
                $(this)
                    .removeClass("fa-solid")
                    .addClass("fa-regular")
                    .css("color", "#666"); // Outlined star color
            }
        });
    }   

    // Save button functionality
    $('#save-button').on('click', function () {
        // Send the rating data to the server
        $.ajax({
            type: "POST",
            url: "/Product/SaveRating", // Adjust the URL according to your route
            data: {
                gameId: gameId, // Pass the game ID from the model
                rating: selectedRating // The selected rating
            },
            success: function (response) {
                if (response.success === false) {
                    alert(response.message); // Show the error message if success is false
                } else {
                    // Replace the rating container content with the updated partial view
                    $('#rating-container').html(response);
                    // Reattach the click event handler for the "Rate" link
                    $('#rate-link').on('click', function () {
                        $('#rateModal').fadeIn(); // reattaching the modal so it works again
                    });
                    $("#rateModal").fadeOut(); 
                }
            },
            error: function () {
                alert("An error occurred while submitting your rating.");
            }
        });

        $('#rateModal').fadeOut(); // Close the modal after saving
        $('#single-star').html('<i class="fa-regular fa-star" style="color: gold;"></i>'); // Reset single star
    });

    // Close the modal when the close button or outside area is clicked (existing code)
    $(".close, #rateModal").on("click", function (e) {
        if ($(e.target).is(".close") || $(e.target).is("#rateModal")) {
            $("#rateModal").fadeOut(); // Hide modal
        }
    });
});
