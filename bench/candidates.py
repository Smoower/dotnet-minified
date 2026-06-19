#!/usr/bin/env python3
"""Token deltas for *candidate* libraries we might wrap next.

Each row is a realistic call-site: (library, long form, compact candidate).
The compact form is hypothetical - the point is to see which wraps actually pay.

Reports approximate per-call savings, rounded to the nearest 5%, for the
tiktoken proxy and (when a key is set) Claude's real tokenizer. Run:

    pip install tiktoken anthropic
    export ANTHROPIC_API_KEY=...        # free count_tokens endpoint
    python bench/candidates.py
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _tokens import (
    CLAUDE_MODEL,
    approx_pct,
    claude_available,
    claude_toks,
    claude_unavailable_reason,
    reduction,
    tiktoken_toks,
)

C = [
 ("System.Text.Json", 'JsonSerializer.Serialize(x)', 'x.toJson()'),
 ("System.Text.Json", 'JsonSerializer.Deserialize<T>(s)', 's.fromJson<T>()'),
 ("Newtonsoft.Json", 'JsonConvert.SerializeObject(x)', 'x.toJson()'),
 ("Newtonsoft.Json", 'JsonConvert.DeserializeObject<T>(s)', 's.fromJson<T>()'),
 ("Newtonsoft.Json", 'JsonConvert.SerializeObject(x, Formatting.Indented)', 'x.toJson(pretty:true)'),

 ("FluentValidation", 'RuleFor(x => x.Name).NotEmpty().MaximumLength(100);', 'req(x=>x.Name).max(100);'),
 ("FluentValidation", 'RuleFor(x => x.Email).NotEmpty().EmailAddress();', 'req(x=>x.Email).email();'),
 ("FluentValidation", 'RuleFor(x => x.Age).GreaterThan(0).LessThanOrEqualTo(120);', 'req(x=>x.Age).gt(0).lte(120);'),

 ("Dapper", 'await conn.QueryAsync<User>(sql, p)', 'await conn.q<User>(sql, p)'),
 ("Dapper", 'await conn.QueryFirstOrDefaultAsync<User>(sql, p)', 'await conn.q1<User>(sql, p)'),
 ("Dapper", 'await conn.ExecuteAsync(sql, p)', 'await conn.ex(sql, p)'),
 ("Dapper", 'await conn.ExecuteScalarAsync<int>(sql, p)', 'await conn.scalar<int>(sql, p)'),

 ("AutoMapper", 'CreateMap<User, UserDto>();', 'map<User, UserDto>();'),
 ("AutoMapper", '_mapper.Map<UserDto>(user)', 'user.to<UserDto>()'),
 ("AutoMapper", 'config.CreateMap<User, UserDto>().ReverseMap();', 'map<User, UserDto>(both:true);'),

 ("MediatR", 'public class GetUser : IRequest<UserDto>', 'public class GetUser : Req<UserDto>'),
 ("MediatR", 'public async Task<UserDto> Handle(GetUser r, CancellationToken ct)', 'public async Task<UserDto> Handle(GetUser r, CT ct)'),
 ("MediatR", 'await _mediator.Send(new GetUser(id))', 'await m.send(new GetUser(id))'),

 ("FluentAssertions", 'result.Should().Be(expected);', 'result.shouldBe(expected);'),
 ("FluentAssertions", 'result.Should().NotBeNull();', 'result.shouldNotBeNull();'),
 ("FluentAssertions", 'act.Should().Throw<InvalidOperationException>();', 'act.shouldThrow<InvalidOperationException>();'),
 ("xUnit", 'Assert.Equal(expected, actual);', 'eq(expected, actual);'),
 ("xUnit", 'Assert.NotNull(result);', 'notNull(result);'),

 ("Polly", 'Policy.Handle<HttpRequestException>().WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1))', 'retry<HttpRequestException>(3, 1)'),

 ("Swashbuckle", '[ProducesResponseType(StatusCodes.Status200OK)]', '[P200]'),
 ("Swashbuckle", '[ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]', '[P200<UserDto>]'),
 ("Swashbuckle", '[ProducesResponseType(StatusCodes.Status404NotFound)]', '[P404]'),

 ("Serilog/ILogger", 'Log.Information("created {Id}", id)', 'Log.inf("created {Id}", id)'),

 ("WPF (DependencyProperty)", 'public static readonly DependencyProperty FooProperty = DependencyProperty.Register(nameof(Foo), typeof(string), typeof(MyControl), new PropertyMetadata(null));', '[Dep] public string Foo { get; set; }'),
 ("WPF/MVVM", 'public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }', '[ObservableProperty] string _name;'),

 ("EF provider setup", 'options.UseSqlServer(connectionString)', 'options.sql(connectionString)'),
 ("EF provider setup", 'options.UseNpgsql(connectionString)', 'options.pg(connectionString)'),
 ("ADO (SqlClient)", 'await using var cmd = new SqlCommand(sql, conn); var r = await cmd.ExecuteReaderAsync();', '// use Dapper instead'),
 ("Polly v8", 'new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3 }).Build()', 'retry(3)'),
 ("OpenTelemetry", 'activity?.SetTag("user.id", id)', 'a?.tag("user.id", id)'),
 ("OpenTelemetry", 'using var activity = ActivitySource.StartActivity("GetUser")', 'using var a = src.span("GetUser")'),
 ("Google.Protobuf", 'GetUserResponse.Parser.ParseFrom(bytes)', 'bytes.proto<GetUserResponse>()'),
 ("Google.Protobuf", 'message.ToByteArray()', 'message.bytes()'),
 ("gRPC client", 'await client.GetUserAsync(new GetUserRequest { Id = id })', 'await client.GetUserAsync(new GetUserRequest { Id = id })'),
]


def mean(xs):
    xs = [x for x in xs if x is not None]
    return sum(xs) / len(xs) if xs else None


use_claude = claude_available()
if use_claude:
    print(f"claude tokenizer: count_tokens / {CLAUDE_MODEL}\n")
    print(f"{'library':26}{'tiktoken':>9}{'claude':>8}  example")
else:
    print(f"claude tokenizer: skipped ({claude_unavailable_reason()})\n")
    print(f"{'library':26}{'tiktoken':>9}  example")

by_lib = {}
for lib, long, short in C:
    tk = reduction(tiktoken_toks(long), tiktoken_toks(short))
    cl = reduction(claude_toks(long), claude_toks(short)) if use_claude else None
    by_lib.setdefault(lib, []).append((tk, cl))
    ex = long if len(long) < 42 else long[:39] + "..."
    if use_claude:
        print(f"{lib:26}{approx_pct(tk):>9}{approx_pct(cl):>8}  {ex}")
    else:
        print(f"{lib:26}{approx_pct(tk):>9}  {ex}")

print("\navg saving per call (approx):")
if use_claude:
    print(f"{'library':26}{'tiktoken':>10}{'claude':>9}")
else:
    print(f"{'library':26}{'tiktoken':>10}")
for lib, rows in by_lib.items():
    tk_avg = approx_pct(mean(r[0] for r in rows))
    if use_claude:
        cl_avg = approx_pct(mean(r[1] for r in rows))
        print(f"{lib:26}{tk_avg:>10}{cl_avg:>9}")
    else:
        print(f"{lib:26}{tk_avg:>10}")

print("\nrounded to the nearest 5% on purpose - report the ballpark, not a precise figure.")
