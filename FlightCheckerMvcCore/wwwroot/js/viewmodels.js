//type definitions
var city = function (code, airportName) {
    var self = this;
    self.code = code;
    self.airportName = airportName;
};

var lookup = function (id, displayText) {
    var self = this;
    self.id = id;
    self.displayText = displayText;
};
//end type definitions

//validation setup
ko.validation.configure({
    registerExtenders: true,
    messagesOnModified: true,
    insertMessages: true,
    parseInputAttributes: true,
    messageTemplate: null
});
//end

//view model definition
function FlightOffersViewModel() {

    // Data
    var self = this;
    self.tabs = ['Flight Offers', 'Saved Offers'];

    self.cabinTypes = ko.observableArray([
        new lookup("economy", "Economy"),
        new lookup("premiumEconomy", "Premium Economy"),
        new lookup("business", "Business"),
        new lookup("first", "First Class")
    ]);
   
    self.journeyTypes = ko.observableArray([
        new lookup("oneWay", "One Way"),
        new lookup("roundTrip", "Round Trip")
    ]);

    self.ranges = ko.observableArray([
        new lookup("yearLow", "Year Low"),
        new lookup("monthLow", "Month Low")
    ]);

    self.arrivalCities = ko.observableArray([
        new city("MAD", "Madrid Barajas"),
        new city("ROM", "Rome"),
        new city("EDI", "Edinburgh Airport"),
        new city("NYC", "New York JFK"),
        new city("PAR", "Paris"),
        new city("BOM", "Mumbai Airport"),
        new city("GLA", "Glasgow International")
    ]);

    self.pageSizeOptions = ko.observableArray([
        new lookup(5, 5),
        new lookup(10, 10)
    ]);

    //initial pagingInfo properties
    self.selectedPageSize = ko.observable();
    self.totalItems = ko.observable();
    self.pageNumber = ko.observable(1);
    self.totalPages = ko.observable(1);
    //end paging properties
    
    self.selectedArrivalCity = ko.observable().extend({ required: true });
    self.selectedRange = ko.observable().extend({ required: true });
    self.selectedJourneyType = ko.observable().extend({ required: true });
    self.selectedCabinType = ko.observable().extend({ required: true });
    self.alertText = ko.observable();

    //tab data selections
    self.chosenTabId = ko.observable();  
    self.chosenTabData = ko.observableArray([]);

    //low fare offer data
    self.savedFlightOffersData = ko.observableArray([]);
    self.chosenFlightOfferData = ko.observable();
    self.ShowFlightOffers = ko.observable(false);

    self.showAlert = ko.observable(false);
 
    //behaviours
    self.clearAlert = function () {
        self.showAlert(false);
        self.alertText("");
    }

    //tabe change event handler
    self.goToTab = function (tab) {
        location.hash = tab;
    };

    //saved flight 'View' button click handler
    self.getFlightOfferData = function (flightOffer) {
        location.hash = self.chosenTabId() + '/' + self.pageNumber() + '/' + flightOffer.id
    };

    //page size change event - routes (using Sammy) back to original in order to rest the page number and take into account the 
    //new page size value
    self.resetPagingInfo = function(pageNumber, pageSize)
    {
        location.hash = self.chosenTabId()
    };

    //paging control button event handler - routers to the required url using sammy
    self.pageSavedFlights = function (pageNumber) {
        location.hash = self.chosenTabId() + '/' + pageNumber
    };

    //adds a low cost offer result to our list of saved offers
    self.addLowestFareFlightOffer = function (flightOffer) {
        $.post("/api/Web/addlowestfareflightoffer", { item: flightOffer }, function () {
            self.chosenTabData.remove(flightOffer); 
            self.goToTab('#Saved Offers');
            self.showAlert(true);
            self.alertText("The entry has been added to your list of Saved Low Cost Offers");
        });
    };

    //deletes a previously saved Low cost flight offer
    self.deleteLowestFareFlightOffer = function (flightOffer) {
        $.post("/api/Web/deletelowestfareflightoffer", { id: flightOffer.id }, function (data) {
            //find entry and remove it from knockout observable array
            self.savedFlightOffersData.remove(flightOffer); 
        });
        
    };

    //search the BA low cost flight offer api
    self.searchLowestFareOffers = function () {
       
        if (self.errors().length > 0) {
            self.errors.showAllMessages();
        }
        else {

            $.get("/api/Web/searchlowestfareflightoffers/"
                + self.selectedArrivalCity() + "/"
                + self.selectedCabinType() + "/"
                + self.selectedJourneyType() + "/"
                + self.selectedRange(), null, function (data) {

                    self.chosenTabData(data);
                    if (self.chosenTabData().length == 0) {
                        self.showAlert(true);
                        self.alertText("No results were returned for the given search criteria");
                    }
                }).fail
                (
                function (jqXHR, textStatus, errorThrown) {

                    processError(jqXHR, textStatus, errorThrown);
                }
                );
        }
    };

    //process response errors
    function processError(jqXHR, textStatus, errorThrown) {

        if (jqXHR.responseText != null)
        {
            var response = $.parseJSON(jqXHR.responseText);
            self.alertText("An unexpected " + + jqXHR.status + " error occurred. Details: " + response.reasonPhrase);
        }
        else
        {
            self.alertText("An unexpected error occurred, httpStatus: " + jqXHR.status);
        }
        self.showAlert(true);
    }

    //url behaviour using Sammy js
    Sammy(function () {
        //route to run for tab only url
        this.get('#:tab', function () {

            self.chosenTabId(this.params.tab);
            self.chosenFlightOfferData(null);
            self.savedFlightOffersData.removeAll();
            self.ShowFlightOffers(false);

            if (this.params.tab == "Flight Offers") {
                self.ShowFlightOffers(true);
            }

            if (this.params.tab == "Saved Offers") {
                //re-route to tab and page 1 url
                location.hash = self.chosenTabId() + '/' + 1;
            }

        });

        //route to run from tab and id in url
        this.get('#:tab/:page', function () {

            self.chosenTabId(this.params.tab);
            self.chosenFlightOfferData(null);
            self.savedFlightOffersData.removeAll();
            self.pageNumber(this.params.page);

            var pagingInfo = {
                pageSize: self.selectedPageSize(),
                pageNumber: self.pageNumber()
            }

            if (this.params.tab == "Saved Offers") { 
                $.post("/api/Web/postsavedflightoffers", pagingInfo, function (data) {
                    self.savedFlightOffersData(data.pricedItineraries);
 
                    self.pageNumber(data.pagingInfo.pageNumber);
                    self.totalItems(data.pagingInfo.totalItems);
                    self.totalPages(data.pagingInfo.totalPages);
                  
                    if (self.savedFlightOffersData().length == 0) {
                        self.showAlert(true);
                        self.alertText("You currently have no saved flights");
                    }
                });
            }
        });

        //route to run from tab and pagenum and id in url
        this.get('#:tab/:page/:id', function () {
            
            self.chosenTabId(this.params.tab);
            self.savedFlightOffersData.removeAll();

            self.ShowFlightOffers(false);
            if (this.params.tab == "Saved Offers") { //gets an indivual saved offer
                $.get("/api/Web/getsavedflightoffer/" + this.params.id, null, self.chosenFlightOfferData);
            }
        });

        //default to run when url is empty
        this.get('', function () {
            this.app.runRoute('get', '#Flight Offers') 
        });

    }).run();

};


//special binding handlers
ko.bindingHandlers.fadeVisible = {
    init: function (element, valueAccessor) {
        // Initially set the element to be instantly visible/hidden depending on the value
        var value = valueAccessor();
        $(element).toggle(ko.unwrap(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
    },
    update: function (element, valueAccessor) {
        // Whenever the value subsequently changes, slowly fade the element in or out
        var value = valueAccessor();
        ko.unwrap(value) ? $(element).fadeIn() : $(element).fadeOut();
    }
};

//set up validation and apply bindings
var viewModel = new FlightOffersViewModel();
viewModel.errors = ko.validation.group(viewModel);
ko.applyBindings(viewModel);


