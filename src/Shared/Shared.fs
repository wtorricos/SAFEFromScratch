namespace Shared

module Helpers =
    let add a b = a + b

type IGreetingApi = {
  greet : string -> Async<string>
}

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName
