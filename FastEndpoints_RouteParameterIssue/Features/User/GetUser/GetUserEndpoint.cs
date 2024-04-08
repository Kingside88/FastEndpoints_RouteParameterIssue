using FastEndpoints;

namespace FastEndpoints_RouteParameterIssue.Features.User.GetUser
{
    sealed class MyRequest
    {
        public int UserId { get; set; } // this int property will correctly make the swagger param int32
    }

    sealed class GetUserEndpoint : Endpoint<MyRequest>
    {
        public override void Configure()
        {
            Get("/users/{UserId}");

            Options(x => x.WithTags("Test"));
            AllowAnonymous();
        }

        public override async Task HandleAsync(MyRequest request, CancellationToken cancellationToken)
        {
            var logger = Resolve<ILogger<GetUserEndpoint>>();

            //var userId = Route<int>("userId");
            int userId = request.UserId;

            var user = new
            {
                UserId = userId,
                FirstName = "John",
                LastName = "Doe",
            };

            if (user is null)
            {
                await SendNotFoundAsync(cancellationToken);
                return;
            }

            await SendAsync(user, cancellation: cancellationToken);
        }
    }
}