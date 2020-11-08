var origin1 = new google.maps.LatLng(55.930385, -3.118425);
var destinationA = new google.maps.LatLng(50.087692, 14.421150);

var service = new google.maps.DistanceMatrixService();


//call the Google Maps Distance Matrix API
//javascript is a pain in the ass....
function getDistance(OriginDestinationMatrix)
{
    console.log("js getdistance");
    var Matrix = JSON.parse(OriginDestinationMatrix);
    var origins = [];
    var destinations = [];
    for (var i in Matrix.origins)
    {
        origins.push(new google.maps.LatLng(Matrix.origins[i].lat, Matrix.origins[i].lng));
    }
    for (var i in Matrix.destinations)
    {
        destinations.push(new google.maps.LatLng(Matrix.destinations[i].lat, Matrix.destinations[i].lng));
    }
    service.getDistanceMatrix(
        {
            origins: origins,
            destinations: destinations,
            travelMode: Matrix.TravelMode,
            unitSystem: google.maps.UnitSystem.METRIC,
            avoidHighways: false,
            avoidTolls: true,
        }, callback);
}

//just pass the result back to C#
function callback(response, status)
{
    console.log("js callback");
    DotNet.invokeMethodAsync("Caelicus", "GoogleMapsDistanceCallback", response);
}
