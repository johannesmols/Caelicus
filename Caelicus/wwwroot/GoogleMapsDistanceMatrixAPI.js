var origin1 = new google.maps.LatLng(55.930385, -3.118425);
var destinationA = new google.maps.LatLng(50.087692, 14.421150);

var service = new google.maps.DistanceMatrixService();


//call the Google Maps Distance Matrix API
//javascript is a pain in the ass....
function getDistance(OriginDestinationMatrix)
{
    var matrix = JSON.parse(OriginDestinationMatrix);
    var origins = [];
    var destinations = [];

    for (var i in matrix.Origins)
    {
        origins.push(new google.maps.LatLng(matrix.Origins[i].Lat, matrix.Origins[i].Lng));
    }

    for (var j in matrix.Destinations)
    {
        destinations.push(new google.maps.LatLng(matrix.Destinations[j].Lat, matrix.Destinations[j].Lng));
    }

    service.getDistanceMatrix(
        {
            origins: origins,
            destinations: destinations,
            travelMode: matrix.TravelMode,
            unitSystem: google.maps.UnitSystem.METRIC,
            avoidHighways: false,
            avoidTolls: true,
        }, callback);
}

//just pass the result back to C#
function callback(response, status)
{
    DotNet.invokeMethodAsync("Caelicus", "GoogleMapsDistanceCallback", response);
}
