type CommandArgs<TCommand extends new (..._args: any) => any> = ConstructorParameters<TCommand>[0];
type Telemetry = {
    /**
     * Upstash-Telemetry-Sdk
     * @example @upstash/redis@v1.1.1
     */
    sdk?: string;
    /**
     * Upstash-Telemetry-Platform
     * @example cloudflare
     */
    platform?: string;
    /**
     * Upstash-Telemetry-Runtime
     * @example node@v18
     */
    runtime?: string;
};
type RedisOptions = {
    /**
     * Automatically try to deserialize the returned data from upstash using `JSON.deserialize`
     *
     * @default true
     */
    automaticDeserialization?: boolean;
    enableTelemetry?: boolean;
};

type CacheSetting = "default" | "force-cache" | "no-cache" | "no-store" | "only-if-cached" | "reload";
type UpstashRequest = {
    path?: string[];
    /**
     * Request body will be serialized to json
     */
    body?: unknown;
};
type UpstashResponse<TResult> = {
    result?: TResult;
    error?: string;
};
interface Requester {
    request: <TResult = unknown>(req: UpstashRequest) => Promise<UpstashResponse<TResult>>;
}
type RetryConfig = false | {
    /**
     * The number of retries to attempt before giving up.
     *
     * @default 5
     */
    retries?: number;
    /**
     * A backoff function receives the current retry cound and returns a number in milliseconds to wait before retrying.
     *
     * @default
     * ```ts
     * Math.exp(retryCount) * 50
     * ```
     */
    backoff?: (retryCount: number) => number;
};
type RequesterConfig = {
    /**
     * Configure the retry behaviour in case of network errors
     */
    retry?: RetryConfig;
    /**
     * Due to the nature of dynamic and custom data, it is possible to write data to redis that is not
     * valid json and will therefore cause errors when deserializing. This used to happen very
     * frequently with non-utf8 data, such as emojis.
     *
     * By default we will therefore encode the data as base64 on the server, before sending it to the
     * client. The client will then decode the base64 data and parse it as utf8.
     *
     * For very large entries, this can add a few milliseconds, so if you are sure that your data is
     * valid utf8, you can disable this behaviour by setting this option to false.
     *
     * Here's what the response body looks like:
     *
     * ```json
     * {
     *  result?: "base64-encoded",
     *  error?: string
     * }
     * ```
     *
     * @default "base64"
     */
    responseEncoding?: false | "base64";
    /**
     * Configure the cache behaviour
     * @default "no-store"
     */
    cache?: CacheSetting;
};

type Serialize = (data: unknown) => string | number | boolean;
type Deserialize<TResult, TData> = (result: TResult) => TData;
type CommandOptions<TResult, TData> = {
    /**
     * Custom deserializer
     */
    deserialize?: (result: TResult) => TData;
    /**
     * Automatically try to deserialize the returned data from upstash using `JSON.deserialize`
     *
     * @default true
     */
    automaticDeserialization?: boolean;
};
/**
 * Command offers default (de)serialization and the exec method to all commands.
 *
 * TData represents what the user will enter or receive,
 * TResult is the raw data returned from upstash, which may need to be transformed or parsed.
 */
declare class Command<TResult, TData> {
    readonly command: (string | number | boolean)[];
    readonly serialize: Serialize;
    readonly deserialize: Deserialize<TResult, TData>;
    /**
     * Create a new command instance.
     *
     * You can define a custom `deserialize` function. By default we try to deserialize as json.
     */
    constructor(command: (string | boolean | number | unknown)[], opts?: CommandOptions<TResult, TData>);
    /**
     * Execute the command using a client.
     */
    exec(client: Requester): Promise<TData>;
}

type ZUnionStoreCommandOptions = {
    aggregate?: "sum" | "min" | "max";
} & ({
    weight: number;
    weights?: never;
} | {
    weight?: never;
    weights: number[];
} | {
    weight?: never;
    weights?: never;
});
/**
 * @see https://redis.io/commands/zunionstore
 */
declare class ZUnionStoreCommand extends Command<number, number> {
    constructor(cmd: [destination: string, numKeys: 1, key: string, opts?: ZUnionStoreCommandOptions], cmdOpts?: CommandOptions<number, number>);
    constructor(cmd: [destination: string, numKeys: number, keys: string[], opts?: ZUnionStoreCommandOptions], cmdOpts?: CommandOptions<number, number>);
}

type ZUnionCommandOptions = {
    withScores?: boolean;
    aggregate?: "sum" | "min" | "max";
} & ({
    weight: number;
    weights?: never;
} | {
    weight?: never;
    weights: number[];
} | {
    weight?: never;
    weights?: never;
});
/**
 * @see https://redis.io/commands/zunion
 */
declare class ZUnionCommand<TData extends unknown[]> extends Command<string[], TData> {
    constructor(cmd: [numKeys: 1, key: string, opts?: ZUnionCommandOptions], cmdOpts?: CommandOptions<string[], TData>);
    constructor(cmd: [numKeys: number, keys: string[], opts?: ZUnionCommandOptions], cmdOpts?: CommandOptions<string[], TData>);
}

type ZInterStoreCommandOptions = {
    aggregate?: "sum" | "min" | "max";
} & ({
    weight: number;
    weights?: never;
} | {
    weight?: never;
    weights: number[];
} | {
    weight?: never;
    weights?: never;
});
/**
 * @see https://redis.io/commands/zInterstore
 */
declare class ZInterStoreCommand extends Command<number, number> {
    constructor(cmd: [destination: string, numKeys: 1, key: string, opts?: ZInterStoreCommandOptions], cmdOpts?: CommandOptions<number, number>);
    constructor(cmd: [destination: string, numKeys: number, keys: string[], opts?: ZInterStoreCommandOptions], cmdOpts?: CommandOptions<number, number>);
}

type Type = "string" | "list" | "set" | "zset" | "hash" | "none";
/**
 * @see https://redis.io/commands/type
 */
declare class TypeCommand extends Command<Type, Type> {
    constructor(cmd: [key: string], opts?: CommandOptions<Type, Type>);
}

type ScriptFlushCommandOptions = {
    sync: true;
    async?: never;
} | {
    sync?: never;
    async: true;
};
/**
 * @see https://redis.io/commands/script-flush
 */
declare class ScriptFlushCommand extends Command<"OK", "OK"> {
    constructor([opts]: [opts?: ScriptFlushCommandOptions], cmdOpts?: CommandOptions<"OK", "OK">);
}

type ScanCommandOptions = {
    match?: string;
    count?: number;
    type?: string;
};
/**
 * @see https://redis.io/commands/scan
 */
declare class ScanCommand extends Command<[number, string[]], [number, string[]]> {
    constructor([cursor, opts]: [cursor: number, opts?: ScanCommandOptions], cmdOpts?: CommandOptions<[number, string[]], [number, string[]]>);
}

type GeoAddCommandOptions = {
    nx?: boolean;
    xx?: never;
} | ({
    nx?: never;
    xx?: boolean;
} & {
    ch?: boolean;
});
interface GeoMember<TMemberType> {
    latitude: number;
    longitude: number;
    member: TMemberType;
}
/**
 * @see https://redis.io/commands/geoadd
 */
declare class GeoAddCommand<TMemberType = string> extends Command<number | null, number | null> {
    constructor([key, arg1, ...arg2]: [
        string,
        GeoMember<TMemberType> | GeoAddCommandOptions,
        ...GeoMember<TMemberType>[]
    ], opts?: CommandOptions<number | null, number | null>);
}

/**
 * @see https://redis.io/commands/append
 */
declare class AppendCommand extends Command<number, number> {
    constructor(cmd: [key: string, value: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/bitcount
 */
declare class BitCountCommand extends Command<number, number> {
    constructor(cmd: [key: string, start?: never, end?: never], opts?: CommandOptions<number, number>);
    constructor(cmd: [key: string, start: number, end: number], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/bitop
 */
declare class BitOpCommand extends Command<number, number> {
    constructor(cmd: [op: "and" | "or" | "xor", destinationKey: string, ...sourceKeys: string[]], opts?: CommandOptions<number, number>);
    constructor(cmd: [op: "not", destinationKey: string, sourceKey: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/bitpos
 */
declare class BitPosCommand extends Command<number, number> {
    constructor(cmd: [key: string, bit: 0 | 1, start?: number, end?: number], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/copy
 */
declare class CopyCommand extends Command<number, "COPIED" | "NOT_COPIED"> {
    constructor([key, destinationKey, opts]: [key: string, destinationKey: string, opts?: {
        replace: boolean;
    }], commandOptions?: CommandOptions<number, "COPIED" | "NOT_COPIED">);
}

/**
 * @see https://redis.io/commands/dbsize
 */
declare class DBSizeCommand extends Command<number, number> {
    constructor(opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/decr
 */
declare class DecrCommand extends Command<number, number> {
    constructor(cmd: [key: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/decrby
 */
declare class DecrByCommand extends Command<number, number> {
    constructor(cmd: [key: string, decrement: number], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/del
 */
declare class DelCommand extends Command<number, number> {
    constructor(cmd: [...keys: string[]], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/echo
 */
declare class EchoCommand extends Command<string, string> {
    constructor(cmd: [message: string], opts?: CommandOptions<string, string>);
}

/**
 * @see https://redis.io/commands/eval
 */
declare class EvalCommand<TArgs extends unknown[], TData> extends Command<unknown, TData> {
    constructor([script, keys, args]: [script: string, keys: string[], args: TArgs], opts?: CommandOptions<unknown, TData>);
}

/**
 * @see https://redis.io/commands/evalsha
 */
declare class EvalshaCommand<TArgs extends unknown[], TData> extends Command<unknown, TData> {
    constructor([sha, keys, args]: [sha: string, keys: string[], args?: TArgs], opts?: CommandOptions<unknown, TData>);
}

/**
 * @see https://redis.io/commands/exists
 */
declare class ExistsCommand extends Command<number, number> {
    constructor(cmd: [...keys: string[]], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/expire
 */
declare class ExpireCommand extends Command<"0" | "1", 0 | 1> {
    constructor(cmd: [key: string, seconds: number], opts?: CommandOptions<"0" | "1", 0 | 1>);
}

/**
 * @see https://redis.io/commands/expireat
 */
declare class ExpireAtCommand extends Command<"0" | "1", 0 | 1> {
    constructor(cmd: [key: string, unix: number], opts?: CommandOptions<"0" | "1", 0 | 1>);
}

/**
 * @see https://redis.io/commands/flushall
 */
declare class FlushAllCommand extends Command<"OK", "OK"> {
    constructor(args?: [{
        async?: boolean;
    }], opts?: CommandOptions<"OK", "OK">);
}

/**
 * @see https://redis.io/commands/flushdb
 */
declare class FlushDBCommand extends Command<"OK", "OK"> {
    constructor([opts]: [opts?: {
        async?: boolean;
    }], cmdOpts?: CommandOptions<"OK", "OK">);
}

/**
 * @see https://redis.io/commands/geodist
 */
declare class GeoDistCommand<TMemberType = string> extends Command<number | null, number | null> {
    constructor([key, member1, member2, unit]: [
        key: string,
        member1: TMemberType,
        member2: TMemberType,
        unit?: "M" | "KM" | "FT" | "MI"
    ], opts?: CommandOptions<number | null, number | null>);
}

type Coordinates = {
    lng: number;
    lat: number;
};
/**
 * @see https://redis.io/commands/geopos
 */
declare class GeoPosCommand<TMember = string> extends Command<(string | null)[][], Coordinates[]> {
    constructor(cmd: [string, ...(TMember[] | TMember[])], opts?: CommandOptions<(string | null)[][], Coordinates[]>);
}

/**
 * @see https://redis.io/commands/geohash
 */
declare class GeoHashCommand<TMember = string> extends Command<(string | null)[], (string | null)[]> {
    constructor(cmd: [string, ...(TMember[] | TMember[])], opts?: CommandOptions<(string | null)[], (string | null)[]>);
}

type RadiusOptions$1 = "M" | "KM" | "FT" | "MI";
type CenterPoint$1<TMemberType> = {
    type: "FROMMEMBER" | "frommember";
    member: TMemberType;
} | {
    type: "FROMLONLAT" | "fromlonlat";
    coordinate: {
        lon: number;
        lat: number;
    };
};
type Shape$1 = {
    type: "BYRADIUS" | "byradius";
    radius: number;
    radiusType: RadiusOptions$1;
} | {
    type: "BYBOX" | "bybox";
    rect: {
        width: number;
        height: number;
    };
    rectType: RadiusOptions$1;
};
type GeoSearchCommandOptions$1 = {
    count?: {
        limit: number;
        any?: boolean;
    };
    withCoord?: boolean;
    withDist?: boolean;
    withHash?: boolean;
};
type OptionMappings = {
    withHash: "hash";
    withCoord: "coord";
    withDist: "dist";
};
type GeoSearchOptions<TOptions> = {
    [K in keyof TOptions as K extends keyof OptionMappings ? OptionMappings[K] : never]: K extends "withHash" ? string : K extends "withCoord" ? {
        long: number;
        lat: number;
    } : K extends "withDist" ? number : never;
};
type GeoSearchResponse<TOptions, TMemberType> = ({
    member: TMemberType;
} & GeoSearchOptions<TOptions>)[];
/**
 * @see https://redis.io/commands/geosearch
 */
declare class GeoSearchCommand<TMemberType = string, TOptions extends GeoSearchCommandOptions$1 = GeoSearchCommandOptions$1> extends Command<any[] | any[][], GeoSearchResponse<TOptions, TMemberType>> {
    constructor([key, centerPoint, shape, order, opts]: [
        key: string,
        centerPoint: CenterPoint$1<TMemberType>,
        shape: Shape$1,
        order: "ASC" | "DESC" | "asc" | "desc",
        opts?: TOptions
    ], commandOptions?: CommandOptions<any[] | any[][], GeoSearchResponse<TOptions, TMemberType>>);
}

type RadiusOptions = "M" | "KM" | "FT" | "MI";
type CenterPoint<TMemberType> = {
    type: "FROMMEMBER" | "frommember";
    member: TMemberType;
} | {
    type: "FROMLONLAT" | "fromlonlat";
    coordinate: {
        lon: number;
        lat: number;
    };
};
type Shape = {
    type: "BYRADIUS" | "byradius";
    radius: number;
    radiusType: RadiusOptions;
} | {
    type: "BYBOX" | "bybox";
    rect: {
        width: number;
        height: number;
    };
    rectType: RadiusOptions;
};
type GeoSearchCommandOptions = {
    count?: {
        limit: number;
        any?: boolean;
    };
    storeDist?: boolean;
};
/**
 * @see https://redis.io/commands/geosearchstore
 */
declare class GeoSearchStoreCommand<TMemberType = string, TOptions extends GeoSearchCommandOptions = GeoSearchCommandOptions> extends Command<any[] | any[][], number> {
    constructor([destination, key, centerPoint, shape, order, opts]: [
        destination: string,
        key: string,
        centerPoint: CenterPoint<TMemberType>,
        shape: Shape,
        order: "ASC" | "DESC" | "asc" | "desc",
        opts?: TOptions
    ], commandOptions?: CommandOptions<any[] | any[][], number>);
}

/**
 * @see https://redis.io/commands/get
 */
declare class GetCommand<TData = string> extends Command<unknown | null, TData | null> {
    constructor(cmd: [key: string], opts?: CommandOptions<unknown | null, TData | null>);
}

/**
 * @see https://redis.io/commands/getbit
 */
declare class GetBitCommand extends Command<"0" | "1", 0 | 1> {
    constructor(cmd: [key: string, offset: number], opts?: CommandOptions<"0" | "1", 0 | 1>);
}

/**
 * @see https://redis.io/commands/getdel
 */
declare class GetDelCommand<TData = string> extends Command<unknown | null, TData | null> {
    constructor(cmd: [key: string], opts?: CommandOptions<unknown | null, TData | null>);
}

/**
 * @see https://redis.io/commands/getrange
 */
declare class GetRangeCommand extends Command<string, string> {
    constructor(cmd: [key: string, start: number, end: number], opts?: CommandOptions<string, string>);
}

/**
 * @see https://redis.io/commands/getset
 */
declare class GetSetCommand<TData = string> extends Command<unknown | null, TData | null> {
    constructor(cmd: [key: string, value: TData], opts?: CommandOptions<unknown | null, TData | null>);
}

/**
 * @see https://redis.io/commands/hdel
 */
declare class HDelCommand extends Command<"0" | "1", 0 | 1> {
    constructor(cmd: [key: string, ...fields: string[]], opts?: CommandOptions<"0" | "1", 0 | 1>);
}

/**
 * @see https://redis.io/commands/hexists
 */
declare class HExistsCommand extends Command<number, number> {
    constructor(cmd: [key: string, field: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/hget
 */
declare class HGetCommand<TData> extends Command<unknown | null, TData | null> {
    constructor(cmd: [key: string, field: string], opts?: CommandOptions<unknown | null, TData | null>);
}

/**
 * @see https://redis.io/commands/hgetall
 */
declare class HGetAllCommand<TData extends Record<string, unknown>> extends Command<unknown | null, TData | null> {
    constructor(cmd: [key: string], opts?: CommandOptions<unknown | null, TData | null>);
}

/**
 * @see https://redis.io/commands/hincrby
 */
declare class HIncrByCommand extends Command<number, number> {
    constructor(cmd: [key: string, field: string, increment: number], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/hincrbyfloat
 */
declare class HIncrByFloatCommand extends Command<number, number> {
    constructor(cmd: [key: string, field: string, increment: number], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/hkeys
 */
declare class HKeysCommand extends Command<string[], string[]> {
    constructor([key]: [key: string], opts?: CommandOptions<string[], string[]>);
}

/**
 * @see https://redis.io/commands/hlen
 */
declare class HLenCommand extends Command<number, number> {
    constructor(cmd: [key: string], opts?: CommandOptions<number, number>);
}

/**
 * hmget returns an object of all requested fields from a hash
 * The field values are returned as an object like this:
 * ```ts
 * {[fieldName: string]: T | null}
 * ```
 *
 * In case the hash does not exist or all fields are empty `null` is returned
 *
 * @see https://redis.io/commands/hmget
 */
declare class HMGetCommand<TData extends Record<string, unknown>> extends Command<(string | null)[], TData | null> {
    constructor([key, ...fields]: [key: string, ...fields: string[]], opts?: CommandOptions<(string | null)[], TData | null>);
}

/**
 * @see https://redis.io/commands/hmset
 */
declare class HMSetCommand<TData> extends Command<"OK", "OK"> {
    constructor([key, kv]: [key: string, kv: {
        [field: string]: TData;
    }], opts?: CommandOptions<"OK", "OK">);
}

/**
 * @see https://redis.io/commands/hrandfield
 */
declare class HRandFieldCommand<TData extends string | string[] | Record<string, unknown>> extends Command<string | string[], TData> {
    constructor(cmd: [key: string], opts?: CommandOptions<string, string>);
    constructor(cmd: [key: string, count: number], opts?: CommandOptions<string[], string[]>);
    constructor(cmd: [key: string, count: number, withValues: boolean], opts?: CommandOptions<string[], Partial<TData>>);
}

/**
 * @see https://redis.io/commands/hscan
 */
declare class HScanCommand extends Command<[
    number,
    (string | number)[]
], [
    number,
    (string | number)[]
]> {
    constructor([key, cursor, cmdOpts]: [key: string, cursor: number, cmdOpts?: ScanCommandOptions], opts?: CommandOptions<[number, (string | number)[]], [number, (string | number)[]]>);
}

/**
 * @see https://redis.io/commands/hset
 */
declare class HSetCommand<TData> extends Command<number, number> {
    constructor([key, kv]: [key: string, kv: {
        [field: string]: TData;
    }], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/hsetnx
 */
declare class HSetNXCommand<TData> extends Command<"0" | "1", 0 | 1> {
    constructor(cmd: [key: string, field: string, value: TData], opts?: CommandOptions<"0" | "1", 0 | 1>);
}

/**
 * @see https://redis.io/commands/hstrlen
 */
declare class HStrLenCommand extends Command<number, number> {
    constructor(cmd: [key: string, field: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/hvals
 */
declare class HValsCommand<TData extends unknown[]> extends Command<unknown[], TData> {
    constructor(cmd: [key: string], opts?: CommandOptions<unknown[], TData>);
}

/**
 * @see https://redis.io/commands/incr
 */
declare class IncrCommand extends Command<number, number> {
    constructor(cmd: [key: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/incrby
 */
declare class IncrByCommand extends Command<number, number> {
    constructor(cmd: [key: string, value: number], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/incrbyfloat
 */
declare class IncrByFloatCommand extends Command<number, number> {
    constructor(cmd: [key: string, value: number], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/json.arrappend
 */
declare class JsonArrAppendCommand<TData extends unknown[]> extends Command<(null | string)[], (null | number)[]> {
    constructor(cmd: [key: string, path: string, ...values: TData], opts?: CommandOptions<(null | string)[], (null | number)[]>);
}

/**
 * @see https://redis.io/commands/json.arrindex
 */
declare class JsonArrIndexCommand<TValue> extends Command<(null | string)[], (null | number)[]> {
    constructor(cmd: [key: string, path: string, value: TValue, start?: number, stop?: number], opts?: CommandOptions<(null | string)[], (null | number)[]>);
}

/**
 * @see https://redis.io/commands/json.arrinsert
 */
declare class JsonArrInsertCommand<TData extends unknown[]> extends Command<(null | string)[], (null | number)[]> {
    constructor(cmd: [key: string, path: string, index: number, ...values: TData], opts?: CommandOptions<(null | string)[], (null | number)[]>);
}

/**
 * @see https://redis.io/commands/json.arrlen
 */
declare class JsonArrLenCommand extends Command<(null | string)[], (null | number)[]> {
    constructor(cmd: [key: string, path?: string], opts?: CommandOptions<(null | string)[], (null | number)[]>);
}

/**
 * @see https://redis.io/commands/json.arrpop
 */
declare class JsonArrPopCommand<TData> extends Command<(null | string)[], (TData | null)[]> {
    constructor(cmd: [key: string, path?: string, index?: number], opts?: CommandOptions<(null | string)[], (TData | null)[]>);
}

/**
 * @see https://redis.io/commands/json.arrtrim
 */
declare class JsonArrTrimCommand extends Command<(null | string)[], (null | number)[]> {
    constructor(cmd: [key: string, path?: string, start?: number, stop?: number], opts?: CommandOptions<(null | string)[], (null | number)[]>);
}

/**
 * @see https://redis.io/commands/json.clear
 */
declare class JsonClearCommand extends Command<number, number> {
    constructor(cmd: [key: string, path?: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/json.del
 */
declare class JsonDelCommand extends Command<number, number> {
    constructor(cmd: [key: string, path?: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/json.forget
 */
declare class JsonForgetCommand extends Command<number, number> {
    constructor(cmd: [key: string, path?: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/json.get
 */
declare class JsonGetCommand<TData extends (unknown | Record<string, unknown>) | (unknown | Record<string, unknown>)[]> extends Command<TData | null, TData | null> {
    constructor(cmd: [
        key: string,
        opts?: {
            indent?: string;
            newline?: string;
            space?: string;
        },
        ...path: string[]
    ] | [key: string, ...path: string[]], opts?: CommandOptions<TData | null, TData | null>);
}

/**
 * @see https://redis.io/commands/json.mget
 */
declare class JsonMGetCommand<TData extends (unknown | Record<string, unknown>)[]> extends Command<TData, TData> {
    constructor(cmd: [keys: string[], path: string], opts?: CommandOptions<TData, TData>);
}

/**
 * @see https://redis.io/commands/json.numincrby
 */
declare class JsonNumIncrByCommand extends Command<(null | string)[], (null | number)[]> {
    constructor(cmd: [key: string, path: string, value: number], opts?: CommandOptions<(null | string)[], (null | number)[]>);
}

/**
 * @see https://redis.io/commands/json.nummultby
 */
declare class JsonNumMultByCommand extends Command<(null | string)[], (null | number)[]> {
    constructor(cmd: [key: string, path: string, value: number], opts?: CommandOptions<(null | string)[], (null | number)[]>);
}

/**
 * @see https://redis.io/commands/json.objkeys
 */
declare class JsonObjKeysCommand extends Command<(string[] | null)[], (string[] | null)[]> {
    constructor(cmd: [key: string, path?: string], opts?: CommandOptions<(string[] | null)[], (string[] | null)[]>);
}

/**
 * @see https://redis.io/commands/json.objlen
 */
declare class JsonObjLenCommand extends Command<(number | null)[], (number | null)[]> {
    constructor(cmd: [key: string, path?: string], opts?: CommandOptions<(number | null)[], (number | null)[]>);
}

/**
 * @see https://redis.io/commands/json.resp
 */
declare class JsonRespCommand<TData extends unknown[]> extends Command<TData, TData> {
    constructor(cmd: [key: string, path?: string], opts?: CommandOptions<TData, TData>);
}

/**
 * @see https://redis.io/commands/json.set
 */
declare class JsonSetCommand<TData extends number | string | boolean | Record<string, unknown> | (number | string | boolean | Record<string, unknown>)[]> extends Command<"OK" | null, "OK" | null> {
    constructor(cmd: [
        key: string,
        path: string,
        value: TData,
        opts?: {
            nx: true;
            xx?: never;
        } | {
            nx?: never;
            xx: true;
        }
    ], opts?: CommandOptions<"OK" | null, "OK" | null>);
}

/**
 * @see https://redis.io/commands/json.strappend
 */
declare class JsonStrAppendCommand extends Command<(null | string)[], (null | number)[]> {
    constructor(cmd: [key: string, path: string, value: string], opts?: CommandOptions<(null | string)[], (null | number)[]>);
}

/**
 * @see https://redis.io/commands/json.strlen
 */
declare class JsonStrLenCommand extends Command<(number | null)[], (number | null)[]> {
    constructor(cmd: [key: string, path?: string], opts?: CommandOptions<(number | null)[], (number | null)[]>);
}

/**
 * @see https://redis.io/commands/json.toggle
 */
declare class JsonToggleCommand extends Command<number[], number[]> {
    constructor(cmd: [key: string, path: string], opts?: CommandOptions<number[], number[]>);
}

/**
 * @see https://redis.io/commands/json.type
 */
declare class JsonTypeCommand extends Command<string[], string[]> {
    constructor(cmd: [key: string, path?: string], opts?: CommandOptions<string[], string[]>);
}

/**
 * @see https://redis.io/commands/keys
 */
declare class KeysCommand extends Command<string[], string[]> {
    constructor(cmd: [pattern: string], opts?: CommandOptions<string[], string[]>);
}

declare class LIndexCommand<TData = string> extends Command<unknown | null, TData | null> {
    constructor(cmd: [key: string, index: number], opts?: CommandOptions<unknown | null, TData | null>);
}

declare class LInsertCommand<TData = string> extends Command<number, number> {
    constructor(cmd: [key: string, direction: "before" | "after", pivot: TData, value: TData], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/llen
 */
declare class LLenCommand extends Command<number, number> {
    constructor(cmd: [key: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/lmove
 */
declare class LMoveCommand<TData = string> extends Command<TData, TData> {
    constructor(cmd: [
        source: string,
        destination: string,
        whereFrom: "left" | "right",
        whereTo: "left" | "right"
    ], opts?: CommandOptions<TData, TData>);
}

/**
 * @see https://redis.io/commands/lpop
 */
declare class LPopCommand<TData = string> extends Command<unknown | null, TData | null> {
    constructor(cmd: [key: string, count?: number], opts?: CommandOptions<unknown | null, TData | null>);
}

/**
 * @see https://redis.io/commands/lpush
 */
declare class LPushCommand<TData = string> extends Command<number, number> {
    constructor(cmd: [key: string, ...elements: TData[]], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/lpushx
 */
declare class LPushXCommand<TData> extends Command<number, number> {
    constructor(cmd: [key: string, ...elements: TData[]], opts?: CommandOptions<number, number>);
}

declare class LRangeCommand<TData = string> extends Command<unknown[], TData[]> {
    constructor(cmd: [key: string, start: number, end: number], opts?: CommandOptions<unknown[], TData[]>);
}

declare class LRemCommand<TData> extends Command<number, number> {
    constructor(cmd: [key: string, count: number, value: TData], opts?: CommandOptions<number, number>);
}

declare class LSetCommand<TData = string> extends Command<"OK", "OK"> {
    constructor(cmd: [key: string, index: number, data: TData], opts?: CommandOptions<"OK", "OK">);
}

declare class LTrimCommand extends Command<"OK", "OK"> {
    constructor(cmd: [key: string, start: number, end: number], opts?: CommandOptions<"OK", "OK">);
}

/**
 * @see https://redis.io/commands/mget
 */
declare class MGetCommand<TData extends unknown[]> extends Command<(string | null)[], TData> {
    constructor(cmd: [string[]] | [...(string[] | string[])], opts?: CommandOptions<(string | null)[], TData>);
}

/**
 * @see https://redis.io/commands/mset
 */
declare class MSetCommand<TData> extends Command<"OK", "OK"> {
    constructor([kv]: [kv: {
        [key: string]: TData;
    }], opts?: CommandOptions<"OK", "OK">);
}

/**
 * @see https://redis.io/commands/msetnx
 */
declare class MSetNXCommand<TData = string> extends Command<number, number> {
    constructor([kv]: [kv: {
        [key: string]: TData;
    }], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/persist
 */
declare class PersistCommand extends Command<"0" | "1", 0 | 1> {
    constructor(cmd: [key: string], opts?: CommandOptions<"0" | "1", 0 | 1>);
}

/**
 * @see https://redis.io/commands/pexpire
 */
declare class PExpireCommand extends Command<"0" | "1", 0 | 1> {
    constructor(cmd: [key: string, milliseconds: number], opts?: CommandOptions<"0" | "1", 0 | 1>);
}

/**
 * @see https://redis.io/commands/pexpireat
 */
declare class PExpireAtCommand extends Command<"0" | "1", 0 | 1> {
    constructor(cmd: [key: string, unix: number], opts?: CommandOptions<"0" | "1", 0 | 1>);
}

/**
 * @see https://redis.io/commands/ping
 */
declare class PingCommand extends Command<string | "PONG", string | "PONG"> {
    constructor(cmd?: [message?: string], opts?: CommandOptions<string | "PONG", string | "PONG">);
}

/**
 * @see https://redis.io/commands/psetex
 */
declare class PSetEXCommand<TData = string> extends Command<string, string> {
    constructor(cmd: [key: string, ttl: number, value: TData], opts?: CommandOptions<string, string>);
}

/**
 * @see https://redis.io/commands/pttl
 */
declare class PTtlCommand extends Command<number, number> {
    constructor(cmd: [key: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/publish
 */
declare class PublishCommand<TMessage = unknown> extends Command<number, number> {
    constructor(cmd: [channel: string, message: TMessage], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/randomkey
 */
declare class RandomKeyCommand extends Command<string | null, string | null> {
    constructor(opts?: CommandOptions<string | null, string | null>);
}

/**
 * @see https://redis.io/commands/rename
 */
declare class RenameCommand extends Command<"OK", "OK"> {
    constructor(cmd: [source: string, destination: string], opts?: CommandOptions<"OK", "OK">);
}

/**
 * @see https://redis.io/commands/renamenx
 */
declare class RenameNXCommand extends Command<"0" | "1", 0 | 1> {
    constructor(cmd: [source: string, destination: string], opts?: CommandOptions<"0" | "1", 0 | 1>);
}

/**
 * @see https://redis.io/commands/rpop
 */
declare class RPopCommand<TData extends unknown | unknown[] = string> extends Command<unknown | null, TData | null> {
    constructor(cmd: [key: string, count?: number], opts?: CommandOptions<unknown | null, TData | null>);
}

/**
 * @see https://redis.io/commands/rpush
 */
declare class RPushCommand<TData = string> extends Command<number, number> {
    constructor(cmd: [key: string, ...elements: TData[]], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/rpushx
 */
declare class RPushXCommand<TData = string> extends Command<number, number> {
    constructor(cmd: [key: string, ...elements: TData[]], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/sadd
 */
declare class SAddCommand<TData = string> extends Command<number, number> {
    constructor(cmd: [key: string, ...members: TData[]], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/scard
 */
declare class SCardCommand extends Command<number, number> {
    constructor(cmd: [key: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/script-exists
 */
declare class ScriptExistsCommand<T extends string[]> extends Command<string[], number[]> {
    constructor(hashes: T, opts?: CommandOptions<string[], number[]>);
}

/**
 * @see https://redis.io/commands/script-load
 */
declare class ScriptLoadCommand extends Command<string, string> {
    constructor(args: [script: string], opts?: CommandOptions<string, string>);
}

/**
 * @see https://redis.io/commands/sdiff
 */
declare class SDiffCommand<TData> extends Command<unknown[], TData[]> {
    constructor(cmd: [key: string, ...keys: string[]], opts?: CommandOptions<unknown[], TData[]>);
}

/**
 * @see https://redis.io/commands/sdiffstore
 */
declare class SDiffStoreCommand extends Command<number, number> {
    constructor(cmd: [destination: string, ...keys: string[]], opts?: CommandOptions<number, number>);
}

type SetCommandOptions = {
    get?: boolean;
} & ({
    ex: number;
    px?: never;
    exat?: never;
    pxat?: never;
    keepTtl?: never;
} | {
    ex?: never;
    px: number;
    exat?: never;
    pxat?: never;
    keepTtl?: never;
} | {
    ex?: never;
    px?: never;
    exat: number;
    pxat?: never;
    keepTtl?: never;
} | {
    ex?: never;
    px?: never;
    exat?: never;
    pxat: number;
    keepTtl?: never;
} | {
    ex?: never;
    px?: never;
    exat?: never;
    pxat?: never;
    keepTtl: true;
} | {
    ex?: never;
    px?: never;
    exat?: never;
    pxat?: never;
    keepTtl?: never;
}) & ({
    nx: true;
    xx?: never;
} | {
    xx: true;
    nx?: never;
} | {
    xx?: never;
    nx?: never;
});
/**
 * @see https://redis.io/commands/set
 */
declare class SetCommand<TData, TResult = TData | "OK" | null> extends Command<TResult, TData | "OK" | null> {
    constructor([key, value, opts]: [key: string, value: TData, opts?: SetCommandOptions], cmdOpts?: CommandOptions<TResult, TData>);
}

/**
 * @see https://redis.io/commands/setbit
 */
declare class SetBitCommand extends Command<"0" | "1", 0 | 1> {
    constructor(cmd: [key: string, offset: number, value: 0 | 1], opts?: CommandOptions<"0" | "1", 0 | 1>);
}

/**
 * @see https://redis.io/commands/setex
 */
declare class SetExCommand<TData = string> extends Command<"OK", "OK"> {
    constructor(cmd: [key: string, ttl: number, value: TData], opts?: CommandOptions<"OK", "OK">);
}

/**
 * @see https://redis.io/commands/setnx
 */
declare class SetNxCommand<TData = string> extends Command<number, number> {
    constructor(cmd: [key: string, value: TData], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/setrange
 */
declare class SetRangeCommand extends Command<number, number> {
    constructor(cmd: [key: string, offset: number, value: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/sinter
 */
declare class SInterCommand<TData = string> extends Command<unknown[], TData[]> {
    constructor(cmd: [key: string, ...keys: string[]], opts?: CommandOptions<unknown[], TData[]>);
}

/**
 * @see https://redis.io/commands/sinterstore
 */
declare class SInterStoreCommand extends Command<number, number> {
    constructor(cmd: [destination: string, key: string, ...keys: string[]], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/sismember
 */
declare class SIsMemberCommand<TData = string> extends Command<"0" | "1", 0 | 1> {
    constructor(cmd: [key: string, member: TData], opts?: CommandOptions<"0" | "1", 0 | 1>);
}

/**
 * @see https://redis.io/commands/smembers
 */
declare class SMembersCommand<TData extends unknown[] = string[]> extends Command<unknown[], TData> {
    constructor(cmd: [key: string], opts?: CommandOptions<unknown[], TData>);
}

/**
 * @see https://redis.io/commands/smismember
 */
declare class SMIsMemberCommand<TMembers extends unknown[]> extends Command<("0" | "1")[], (0 | 1)[]> {
    constructor(cmd: [key: string, members: TMembers], opts?: CommandOptions<("0" | "1")[], (0 | 1)[]>);
}

/**
 * @see https://redis.io/commands/smove
 */
declare class SMoveCommand<TData> extends Command<"0" | "1", 0 | 1> {
    constructor(cmd: [source: string, destination: string, member: TData], opts?: CommandOptions<"0" | "1", 0 | 1>);
}

/**
 * @see https://redis.io/commands/spop
 */
declare class SPopCommand<TData> extends Command<string | string[] | null, TData | null> {
    constructor([key, count]: [key: string, count?: number], opts?: CommandOptions<string | string[] | null, TData | null>);
}

/**
 * @see https://redis.io/commands/srandmember
 */
declare class SRandMemberCommand<TData> extends Command<string | null, TData | null> {
    constructor([key, count]: [key: string, count?: number], opts?: CommandOptions<string | null, TData | null>);
}

/**
 * @see https://redis.io/commands/srem
 */
declare class SRemCommand<TData = string> extends Command<number, number> {
    constructor(cmd: [key: string, ...members: TData[]], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/sscan
 */
declare class SScanCommand extends Command<[
    number,
    (string | number)[]
], [
    number,
    (string | number)[]
]> {
    constructor([key, cursor, opts]: [key: string, cursor: number, opts?: ScanCommandOptions], cmdOpts?: CommandOptions<[number, (string | number)[]], [number, (string | number)[]]>);
}

/**
 * @see https://redis.io/commands/strlen
 */
declare class StrLenCommand extends Command<number, number> {
    constructor(cmd: [key: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/sunion
 */
declare class SUnionCommand<TData> extends Command<string[], TData[]> {
    constructor(cmd: [key: string, ...keys: string[]], opts?: CommandOptions<string[], TData[]>);
}

/**
 * @see https://redis.io/commands/sunionstore
 */
declare class SUnionStoreCommand extends Command<number, number> {
    constructor(cmd: [destination: string, key: string, ...keys: string[]], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/time
 */
declare class TimeCommand extends Command<[number, number], [number, number]> {
    constructor(opts?: CommandOptions<[number, number], [number, number]>);
}

/**
 * @see https://redis.io/commands/touch
 */
declare class TouchCommand extends Command<number, number> {
    constructor(cmd: [...keys: string[]], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/ttl
 */
declare class TtlCommand extends Command<number, number> {
    constructor(cmd: [key: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/unlink
 */
declare class UnlinkCommand extends Command<number, number> {
    constructor(cmd: [...keys: string[]], opts?: CommandOptions<number, number>);
}

type XAddCommandOptions = {
    nomkStream?: boolean;
    trim?: ({
        type: "MAXLEN" | "maxlen";
        threshold: number;
    } | {
        type: "MINID" | "minid";
        threshold: string;
    }) & ({
        comparison: "~";
        limit?: number;
    } | {
        comparison: "=";
        limit?: never;
    });
};
/**
 * @see https://redis.io/commands/xadd
 */
declare class XAddCommand extends Command<string, string> {
    constructor([key, id, entries, opts]: [
        key: string,
        id: "*" | string,
        entries: {
            [field: string]: unknown;
        },
        opts?: XAddCommandOptions
    ], commandOptions?: CommandOptions<string, string>);
}

declare class XRangeCommand<TData extends Record<string, Record<string, unknown>>> extends Command<string[][], TData> {
    constructor([key, start, end, count]: [key: string, start: string, end: string, count?: number], opts?: CommandOptions<unknown[], TData[]>);
}

type ZAddCommandOptions = ({
    nx: true;
    xx?: never;
} | {
    nx?: never;
    xx: true;
} | {
    nx?: never;
    xx?: never;
}) & {
    ch?: true;
};
type ZAddCommandOptionsWithIncr = ZAddCommandOptions & {
    incr: true;
};
type ScoreMember<TData> = {
    score: number;
    member: TData;
};
/**
 * @see https://redis.io/commands/zadd
 */
declare class ZAddCommand<TData = string> extends Command<number | null, number | null> {
    constructor(cmd: [key: string, scoreMember: ScoreMember<TData>, ...scoreMemberPairs: ScoreMember<TData>[]], opts?: CommandOptions<number | null, number | null>);
    constructor(cmd: [
        key: string,
        opts: ZAddCommandOptions | ZAddCommandOptionsWithIncr,
        ...scoreMemberPairs: ScoreMember<TData>[]
    ], opts?: CommandOptions<number | null, number | null>);
}

/**
 * @see https://redis.io/commands/zcard
 */
declare class ZCardCommand extends Command<number, number> {
    constructor(cmd: [key: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/zcount
 */
declare class ZCountCommand extends Command<number, number> {
    constructor(cmd: [key: string, min: number | string, max: number | string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/zincrby
 */
declare class ZIncrByCommand<TData> extends Command<number, number> {
    constructor(cmd: [key: string, increment: number, member: TData], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/zlexcount
 */
declare class ZLexCountCommand extends Command<number, number> {
    constructor(cmd: [key: string, min: string, max: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/zpopmax
 */
declare class ZPopMaxCommand<TData> extends Command<string[], TData[]> {
    constructor([key, count]: [key: string, count?: number], opts?: CommandOptions<string[], TData[]>);
}

/**
 * @see https://redis.io/commands/zpopmin
 */
declare class ZPopMinCommand<TData> extends Command<string[], TData[]> {
    constructor([key, count]: [key: string, count?: number], opts?: CommandOptions<string[], TData[]>);
}

type ZRangeCommandOptions = {
    withScores?: boolean;
    rev?: boolean;
} & ({
    byScore: true;
    byLex?: never;
} | {
    byScore?: never;
    byLex: true;
} | {
    byScore?: never;
    byLex?: never;
}) & ({
    offset: number;
    count: number;
} | {
    offset?: never;
    count?: never;
});
/**
 * @see https://redis.io/commands/zrange
 */
declare class ZRangeCommand<TData extends unknown[]> extends Command<string[], TData> {
    constructor(cmd: [key: string, min: number, max: number, opts?: ZRangeCommandOptions], cmdOpts?: CommandOptions<string[], TData>);
    constructor(cmd: [
        key: string,
        min: `(${string}` | `[${string}` | "-" | "+",
        max: `(${string}` | `[${string}` | "-" | "+",
        opts: {
            byLex: true;
        } & ZRangeCommandOptions
    ], cmdOpts?: CommandOptions<string[], TData>);
    constructor(cmd: [
        key: string,
        min: number | `(${number}` | "-inf" | "+inf",
        max: number | `(${number}` | "-inf" | "+inf",
        opts: {
            byScore: true;
        } & ZRangeCommandOptions
    ], cmdOpts?: CommandOptions<string[], TData>);
}

/**
 *  @see https://redis.io/commands/zrank
 */
declare class ZRankCommand<TData> extends Command<number | null, number | null> {
    constructor(cmd: [key: string, member: TData], opts?: CommandOptions<number | null, number | null>);
}

/**
 * @see https://redis.io/commands/zrem
 */
declare class ZRemCommand<TData = string> extends Command<number, number> {
    constructor(cmd: [key: string, ...members: TData[]], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/zremrangebylex
 */
declare class ZRemRangeByLexCommand extends Command<number, number> {
    constructor(cmd: [key: string, min: string, max: string], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/zremrangebyrank
 */
declare class ZRemRangeByRankCommand extends Command<number, number> {
    constructor(cmd: [key: string, start: number, stop: number], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/zremrangebyscore
 */
declare class ZRemRangeByScoreCommand extends Command<number, number> {
    constructor(cmd: [key: string, min: number, max: number], opts?: CommandOptions<number, number>);
}

/**
 *  @see https://redis.io/commands/zrevrank
 */
declare class ZRevRankCommand<TData> extends Command<number | null, number | null> {
    constructor(cmd: [key: string, member: TData], opts?: CommandOptions<number | null, number | null>);
}

/**
 * @see https://redis.io/commands/zscan
 */
declare class ZScanCommand extends Command<[
    number,
    (string | number)[]
], [
    number,
    (string | number)[]
]> {
    constructor([key, cursor, opts]: [key: string, cursor: number, opts?: ScanCommandOptions], cmdOpts?: CommandOptions<[number, (string | number)[]], [number, (string | number)[]]>);
}

/**
 * @see https://redis.io/commands/zscore
 */
declare class ZScoreCommand<TData> extends Command<string | null, number | null> {
    constructor(cmd: [key: string, member: TData], opts?: CommandOptions<string | null, number | null>);
}

type InferResponseData<T extends unknown[]> = {
    [K in keyof T]: T[K] extends Command<any, infer TData> ? TData : unknown;
};
/**
 * Upstash REST API supports command pipelining to send multiple commands in
 * batch, instead of sending each command one by one and waiting for a response.
 * When using pipelines, several commands are sent using a single HTTP request,
 * and a single JSON array response is returned. Each item in the response array
 * corresponds to the command in the same order within the pipeline.
 *
 * **NOTE:**
 *
 * Execution of the pipeline is not atomic. Even though each command in
 * the pipeline will be executed in order, commands sent by other clients can
 * interleave with the pipeline.
 *
 * **Examples:**
 *
 * ```ts
 *  const p = redis.pipeline() // or redis.multi()
 * p.set("key","value")
 * p.get("key")
 * const res = await p.exec()
 * ```
 *
 * You can also chain commands together
 * ```ts
 * const p = redis.pipeline()
 * const res = await p.set("key","value").get("key").exec()
 * ```
 *
 * Return types are inferred if all commands are chained, but you can still
 * override the response type manually:
 * ```ts
 *  redis.pipeline()
 *   .set("key", { greeting: "hello"})
 *   .get("key")
 *   .exec<["OK", { greeting: string } ]>()
 *
 * ```
 */
declare class Pipeline<TCommands extends Command<any, any>[] = []> {
    private client;
    private commands;
    private commandOptions?;
    private multiExec;
    constructor(opts: {
        client: Requester;
        commandOptions?: CommandOptions<any, any>;
        multiExec?: boolean;
    });
    /**
     * Send the pipeline request to upstash.
     *
     * Returns an array with the results of all pipelined commands.
     *
     * If all commands are statically chained from start to finish, types are inferred. You can still define a return type manually if necessary though:
     * ```ts
     * const p = redis.pipeline()
     * p.get("key")
     * const result = p.exec<[{ greeting: string }]>()
     * ```
     */
    exec: <TCommandResults extends unknown[] = [] extends TCommands ? unknown[] : InferResponseData<TCommands>>() => Promise<TCommandResults>;
    /**
     * Returns the length of pipeline before the execution
     */
    length(): number;
    /**
     * Pushes a command into the pipeline and returns a chainable instance of the
     * pipeline
     */
    private chain;
    /**
     * @see https://redis.io/commands/append
     */
    append: (key: string, value: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/bitcount
     */
    bitcount: (key: string, start: number, end: number) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/bitop
     */
    bitop: {
        (op: "and" | "or" | "xor", destinationKey: string, sourceKey: string, ...sourceKeys: string[]): Pipeline<[...TCommands, BitOpCommand]>;
        (op: "not", destinationKey: string, sourceKey: string): Pipeline<[...TCommands, BitOpCommand]>;
    };
    /**
     * @see https://redis.io/commands/bitpos
     */
    bitpos: (key: string, bit: 0 | 1, start?: number | undefined, end?: number | undefined) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/copy
     */
    copy: (key: string, destinationKey: string, opts?: {
        replace: boolean;
    } | undefined) => Pipeline<[...TCommands, Command<any, "COPIED" | "NOT_COPIED">]>;
    /**
     * @see https://redis.io/commands/zdiffstore
     */
    zdiffstore: (destination: string, numkeys: number, ...keys: string[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/dbsize
     */
    dbsize: () => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/decr
     */
    decr: (key: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/decrby
     */
    decrby: (key: string, decrement: number) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/del
     */
    del: (...args: CommandArgs<typeof DelCommand>) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/echo
     */
    echo: (message: string) => Pipeline<[...TCommands, Command<any, string>]>;
    /**
     * @see https://redis.io/commands/eval
     */
    eval: <TArgs extends unknown[], TData = unknown>(script: string, keys: string[], args: TArgs) => Pipeline<[...TCommands, Command<any, TData>]>;
    /**
     * @see https://redis.io/commands/evalsha
     */
    evalsha: <TArgs extends unknown[], TData = unknown>(sha1: string, keys: string[], args: TArgs) => Pipeline<[...TCommands, Command<any, TData>]>;
    /**
     * @see https://redis.io/commands/exists
     */
    exists: (...args: CommandArgs<typeof ExistsCommand>) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/expire
     */
    expire: (key: string, seconds: number) => Pipeline<[...TCommands, Command<any, 0 | 1>]>;
    /**
     * @see https://redis.io/commands/expireat
     */
    expireat: (key: string, unix: number) => Pipeline<[...TCommands, Command<any, 0 | 1>]>;
    /**
     * @see https://redis.io/commands/flushall
     */
    flushall: (args?: CommandArgs<typeof FlushAllCommand>) => Pipeline<[...TCommands, Command<any, "OK">]>;
    /**
     * @see https://redis.io/commands/flushdb
     */
    flushdb: (opts?: {
        async?: boolean | undefined;
    } | undefined) => Pipeline<[...TCommands, Command<any, "OK">]>;
    /**
     * @see https://redis.io/commands/get
     */
    get: <TData>(key: string) => Pipeline<[...TCommands, Command<any, TData | null>]>;
    /**
     * @see https://redis.io/commands/getbit
     */
    getbit: (key: string, offset: number) => Pipeline<[...TCommands, Command<any, 0 | 1>]>;
    /**
     * @see https://redis.io/commands/getdel
     */
    getdel: <TData>(key: string) => Pipeline<[...TCommands, Command<any, TData | null>]>;
    /**
     * @see https://redis.io/commands/getrange
     */
    getrange: (key: string, start: number, end: number) => Pipeline<[...TCommands, Command<any, string>]>;
    /**
     * @see https://redis.io/commands/getset
     */
    getset: <TData>(key: string, value: TData) => Pipeline<[...TCommands, Command<any, TData | null>]>;
    /**
     * @see https://redis.io/commands/hdel
     */
    hdel: (key: string, ...fields: string[]) => Pipeline<[...TCommands, Command<any, 0 | 1>]>;
    /**
     * @see https://redis.io/commands/hexists
     */
    hexists: (key: string, field: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/hget
     */
    hget: <TData>(key: string, field: string) => Pipeline<[...TCommands, Command<any, TData | null>]>;
    /**
     * @see https://redis.io/commands/hgetall
     */
    hgetall: <TData extends Record<string, unknown>>(key: string) => Pipeline<[...TCommands, Command<any, TData | null>]>;
    /**
     * @see https://redis.io/commands/hincrby
     */
    hincrby: (key: string, field: string, increment: number) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/hincrbyfloat
     */
    hincrbyfloat: (key: string, field: string, increment: number) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/hkeys
     */
    hkeys: (key: string) => Pipeline<[...TCommands, Command<any, string[]>]>;
    /**
     * @see https://redis.io/commands/hlen
     */
    hlen: (key: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/hmget
     */
    hmget: <TData extends Record<string, unknown>>(key: string, ...fields: string[]) => Pipeline<[...TCommands, Command<any, TData | null>]>;
    /**
     * @see https://redis.io/commands/hmset
     */
    hmset: <TData>(key: string, kv: {
        [field: string]: TData;
    }) => Pipeline<[...TCommands, Command<any, "OK">]>;
    /**
     * @see https://redis.io/commands/hrandfield
     */
    hrandfield: <TData extends string | string[] | Record<string, unknown>>(key: string, count?: number, withValues?: boolean) => Pipeline<[...TCommands, Command<any, TData>]>;
    /**
     * @see https://redis.io/commands/hscan
     */
    hscan: (key: string, cursor: number, cmdOpts?: ScanCommandOptions | undefined) => Pipeline<[...TCommands, Command<any, [number, (string | number)[]]>]>;
    /**
     * @see https://redis.io/commands/hset
     */
    hset: <TData>(key: string, kv: {
        [field: string]: TData;
    }) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/hsetnx
     */
    hsetnx: <TData>(key: string, field: string, value: TData) => Pipeline<[...TCommands, Command<any, 0 | 1>]>;
    /**
     * @see https://redis.io/commands/hstrlen
     */
    hstrlen: (key: string, field: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/hvals
     */
    hvals: (key: string) => Pipeline<[...TCommands, Command<any, any>]>;
    /**
     * @see https://redis.io/commands/incr
     */
    incr: (key: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/incrby
     */
    incrby: (key: string, value: number) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/incrbyfloat
     */
    incrbyfloat: (key: string, value: number) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/keys
     */
    keys: (pattern: string) => Pipeline<[...TCommands, Command<any, string[]>]>;
    /**
     * @see https://redis.io/commands/lindex
     */
    lindex: (key: string, index: number) => Pipeline<[...TCommands, Command<any, any>]>;
    /**
     * @see https://redis.io/commands/linsert
     */
    linsert: <TData>(key: string, direction: "before" | "after", pivot: TData, value: TData) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/llen
     */
    llen: (key: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/lmove
     */
    lmove: <TData = string>(source: string, destination: string, whereFrom: "left" | "right", whereTo: "left" | "right") => Pipeline<[...TCommands, Command<any, TData>]>;
    /**
     * @see https://redis.io/commands/lpop
     */
    lpop: <TData>(key: string, count?: number | undefined) => Pipeline<[...TCommands, Command<any, TData | null>]>;
    /**
     * @see https://redis.io/commands/lpos
     */
    lpos: <TData>(key: string, element: unknown, opts?: {
        rank?: number | undefined;
        count?: number | undefined;
        maxLen?: number | undefined;
    } | undefined) => Pipeline<[...TCommands, Command<any, TData>]>;
    /**
     * @see https://redis.io/commands/lpush
     */
    lpush: <TData>(key: string, ...elements: TData[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/lpushx
     */
    lpushx: <TData>(key: string, ...elements: TData[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/lrange
     */
    lrange: <TResult = string>(key: string, start: number, end: number) => Pipeline<[...TCommands, Command<any, TResult[]>]>;
    /**
     * @see https://redis.io/commands/lrem
     */
    lrem: <TData>(key: string, count: number, value: TData) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/lset
     */
    lset: <TData>(key: string, index: number, value: TData) => Pipeline<[...TCommands, Command<any, "OK">]>;
    /**
     * @see https://redis.io/commands/ltrim
     */
    ltrim: (key: string, start: number, end: number) => Pipeline<[...TCommands, Command<any, "OK">]>;
    /**
     * @see https://redis.io/commands/mget
     */
    mget: <TData extends unknown[]>(...args: CommandArgs<typeof MGetCommand>) => Pipeline<[...TCommands, Command<any, TData>]>;
    /**
     * @see https://redis.io/commands/mset
     */
    mset: <TData>(kv: {
        [key: string]: TData;
    }) => Pipeline<[...TCommands, Command<any, "OK">]>;
    /**
     * @see https://redis.io/commands/msetnx
     */
    msetnx: <TData>(kv: {
        [key: string]: TData;
    }) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/persist
     */
    persist: (key: string) => Pipeline<[...TCommands, Command<any, 0 | 1>]>;
    /**
     * @see https://redis.io/commands/pexpire
     */
    pexpire: (key: string, milliseconds: number) => Pipeline<[...TCommands, Command<any, 0 | 1>]>;
    /**
     * @see https://redis.io/commands/pexpireat
     */
    pexpireat: (key: string, unix: number) => Pipeline<[...TCommands, Command<any, 0 | 1>]>;
    /**
   * @see https://redis.io/commands/pfadd
   */
    pfadd: (args_0: string, ...args_1: unknown[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/pfcount
     */
    pfcount: (args_0: string, ...args_1: string[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/pfmerge
     */
    pfmerge: (destination_key: string, ...args_1: string[]) => Pipeline<[...TCommands, Command<any, "OK">]>;
    /**
     * @see https://redis.io/commands/ping
     */
    ping: (args?: CommandArgs<typeof PingCommand>) => Pipeline<[...TCommands, Command<any, string>]>;
    /**
     * @see https://redis.io/commands/psetex
     */
    psetex: <TData>(key: string, ttl: number, value: TData) => Pipeline<[...TCommands, Command<any, string>]>;
    /**
     * @see https://redis.io/commands/pttl
     */
    pttl: (key: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/publish
     */
    publish: (channel: string, message: unknown) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/randomkey
     */
    randomkey: () => Pipeline<[...TCommands, Command<any, string | null>]>;
    /**
     * @see https://redis.io/commands/rename
     */
    rename: (source: string, destination: string) => Pipeline<[...TCommands, Command<any, "OK">]>;
    /**
     * @see https://redis.io/commands/renamenx
     */
    renamenx: (source: string, destination: string) => Pipeline<[...TCommands, Command<any, 0 | 1>]>;
    /**
     * @see https://redis.io/commands/rpop
     */
    rpop: <TData = string>(key: string, count?: number | undefined) => Pipeline<[...TCommands, Command<any, TData | null>]>;
    /**
     * @see https://redis.io/commands/rpush
     */
    rpush: <TData>(key: string, ...elements: TData[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/rpushx
     */
    rpushx: <TData>(key: string, ...elements: TData[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/sadd
     */
    sadd: <TData>(key: string, ...members: TData[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/scan
     */
    scan: (cursor: number, opts?: ScanCommandOptions | undefined) => Pipeline<[...TCommands, Command<any, [number, string[]]>]>;
    /**
     * @see https://redis.io/commands/scard
     */
    scard: (key: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/script-exists
     */
    scriptExists: (...args: CommandArgs<typeof ScriptExistsCommand>) => Pipeline<[...TCommands, Command<any, number[]>]>;
    /**
     * @see https://redis.io/commands/script-flush
     */
    scriptFlush: (opts?: ScriptFlushCommandOptions | undefined) => Pipeline<[...TCommands, Command<any, "OK">]>;
    /**
     * @see https://redis.io/commands/script-load
     */
    scriptLoad: (script: string) => Pipeline<[...TCommands, Command<any, string>]>;
    sdiff: (key: string, ...keys: string[]) => Pipeline<[...TCommands, Command<any, unknown[]>]>;
    /**
     * @see https://redis.io/commands/sdiffstore
     */
    sdiffstore: (destination: string, ...keys: string[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/set
     */
    set: <TData>(key: string, value: TData, opts?: SetCommandOptions) => Pipeline<[...TCommands, Command<any, "OK" | TData | null>]>;
    /**
     * @see https://redis.io/commands/setbit
     */
    setbit: (key: string, offset: number, value: 0 | 1) => Pipeline<[...TCommands, Command<any, 0 | 1>]>;
    /**
     * @see https://redis.io/commands/setex
     */
    setex: <TData>(key: string, ttl: number, value: TData) => Pipeline<[...TCommands, Command<any, "OK">]>;
    /**
     * @see https://redis.io/commands/setnx
     */
    setnx: <TData>(key: string, value: TData) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/setrange
     */
    setrange: (key: string, offset: number, value: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/sinter
     */
    sinter: (key: string, ...keys: string[]) => Pipeline<[...TCommands, Command<any, string[]>]>;
    /**
     * @see https://redis.io/commands/sinterstore
     */
    sinterstore: (destination: string, key: string, ...keys: string[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/sismember
     */
    sismember: <TData>(key: string, member: TData) => Pipeline<[...TCommands, Command<any, 0 | 1>]>;
    /**
     * @see https://redis.io/commands/smembers
     */
    smembers: <TData extends unknown[] = string[]>(key: string) => Pipeline<[...TCommands, Command<any, TData>]>;
    /**
     * @see https://redis.io/commands/smismember
     */
    smismember: <TMembers extends unknown[]>(key: string, members: TMembers) => Pipeline<[...TCommands, Command<any, (0 | 1)[]>]>;
    /**
     * @see https://redis.io/commands/smove
     */
    smove: <TData>(source: string, destination: string, member: TData) => Pipeline<[...TCommands, Command<any, 0 | 1>]>;
    /**
     * @see https://redis.io/commands/spop
     */
    spop: <TData>(key: string, count?: number | undefined) => Pipeline<[...TCommands, Command<any, TData | null>]>;
    /**
     * @see https://redis.io/commands/srandmember
     */
    srandmember: <TData>(key: string, count?: number | undefined) => Pipeline<[...TCommands, Command<any, TData | null>]>;
    /**
     * @see https://redis.io/commands/srem
     */
    srem: <TData>(key: string, ...members: TData[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/sscan
     */
    sscan: (key: string, cursor: number, opts?: ScanCommandOptions | undefined) => Pipeline<[...TCommands, Command<any, [number, (string | number)[]]>]>;
    /**
     * @see https://redis.io/commands/strlen
     */
    strlen: (key: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/sunion
     */
    sunion: (key: string, ...keys: string[]) => Pipeline<[...TCommands, Command<any, unknown[]>]>;
    /**
     * @see https://redis.io/commands/sunionstore
     */
    sunionstore: (destination: string, key: string, ...keys: string[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/time
     */
    time: () => Pipeline<[...TCommands, Command<any, [number, number]>]>;
    /**
     * @see https://redis.io/commands/touch
     */
    touch: (...args: CommandArgs<typeof TouchCommand>) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/ttl
     */
    ttl: (key: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/type
     */
    type: (key: string) => Pipeline<[...TCommands, Command<any, Type>]>;
    /**
     * @see https://redis.io/commands/unlink
     */
    unlink: (...args: CommandArgs<typeof UnlinkCommand>) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/zadd
     */
    zadd: <TData>(...args: [key: string, scoreMember: ScoreMember<TData>, ...scoreMemberPairs: ScoreMember<TData>[]] | [key: string, opts: ZAddCommandOptions | ZAddCommandOptionsWithIncr, ScoreMember<TData>, ...ScoreMember<TData>[]]) => Pipeline<[...TCommands, Command<any, number | null>]>;
    /**
     * @see https://redis.io/commands/zcard
     */
    zcard: (key: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/zcount
     */
    zcount: (key: string, min: string | number, max: string | number) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/zincrby
     */
    zincrby: <TData>(key: string, increment: number, member: TData) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/zinterstore
     */
    zinterstore: (destination: string, numKeys: number, keys: string[], opts?: ZInterStoreCommandOptions | undefined) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/zlexcount
     */
    zlexcount: (key: string, min: string, max: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/zmscore
     */
    zmscore: (key: string, members: unknown[]) => Pipeline<[...TCommands, Command<any, number[] | null>]>;
    /**
     * @see https://redis.io/commands/zpopmax
     */
    zpopmax: <TData>(key: string, count?: number | undefined) => Pipeline<[...TCommands, Command<any, TData[]>]>;
    /**
     * @see https://redis.io/commands/zpopmin
     */
    zpopmin: <TData>(key: string, count?: number | undefined) => Pipeline<[...TCommands, Command<any, TData[]>]>;
    /**
     * @see https://redis.io/commands/zrange
     */
    zrange: <TData extends unknown[]>(...args: [key: string, min: number, max: number, opts?: ZRangeCommandOptions] | [
        key: string,
        min: `(${string}` | `[${string}` | "-" | "+",
        max: `(${string}` | `[${string}` | "-" | "+",
        opts: {
            byLex: true;
        } & ZRangeCommandOptions
    ] | [
        key: string,
        min: number | `(${number}` | "-inf" | "+inf",
        max: number | `(${number}` | "-inf" | "+inf",
        opts: {
            byScore: true;
        } & ZRangeCommandOptions
    ]) => Pipeline<[...TCommands, Command<any, TData>]>;
    /**
     * @see https://redis.io/commands/zrank
     */
    zrank: <TData>(key: string, member: TData) => Pipeline<[...TCommands, Command<any, number | null>]>;
    /**
     * @see https://redis.io/commands/zrem
     */
    zrem: <TData>(key: string, ...members: TData[]) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/zremrangebylex
     */
    zremrangebylex: (key: string, min: string, max: string) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/zremrangebyrank
     */
    zremrangebyrank: (key: string, start: number, stop: number) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/zremrangebyscore
     */
    zremrangebyscore: (key: string, min: number, max: number) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/zrevrank
     */
    zrevrank: <TData>(key: string, member: TData) => Pipeline<[...TCommands, Command<any, number | null>]>;
    /**
     * @see https://redis.io/commands/zscan
     */
    zscan: (key: string, cursor: number, opts?: ScanCommandOptions | undefined) => Pipeline<[...TCommands, Command<any, [number, (string | number)[]]>]>;
    /**
     * @see https://redis.io/commands/zscore
     */
    zscore: <TData>(key: string, member: TData) => Pipeline<[...TCommands, Command<any, number | null>]>;
    /**
     * @see https://redis.io/commands/zunionstore
     */
    zunionstore: (destination: string, numKeys: number, keys: string[], opts?: ZUnionStoreCommandOptions | undefined) => Pipeline<[...TCommands, Command<any, number>]>;
    /**
     * @see https://redis.io/commands/zunion
     */
    zunion: (numKeys: number, keys: string[], opts?: ZUnionCommandOptions | undefined) => Pipeline<[...TCommands, Command<any, any>]>;
    /**
     * @see https://redis.io/commands/?group=json
     */
    get json(): {
        /**
         * @see https://redis.io/commands/json.arrappend
         */
        arrappend: (key: string, path: string, ...values: unknown[]) => Pipeline<[...TCommands, Command<any, (number | null)[]>]>;
        /**
         * @see https://redis.io/commands/json.arrindex
         */
        arrindex: (key: string, path: string, value: unknown, start?: number | undefined, stop?: number | undefined) => Pipeline<[...TCommands, Command<any, (number | null)[]>]>;
        /**
         * @see https://redis.io/commands/json.arrinsert
         */
        arrinsert: (key: string, path: string, index: number, ...values: unknown[]) => Pipeline<[...TCommands, Command<any, (number | null)[]>]>;
        /**
         * @see https://redis.io/commands/json.arrlen
         */
        arrlen: (key: string, path?: string | undefined) => Pipeline<[...TCommands, Command<any, (number | null)[]>]>;
        /**
         * @see https://redis.io/commands/json.arrpop
         */
        arrpop: (key: string, path?: string | undefined, index?: number | undefined) => Pipeline<[...TCommands, Command<any, unknown[]>]>;
        /**
         * @see https://redis.io/commands/json.arrtrim
         */
        arrtrim: (key: string, path?: string | undefined, start?: number | undefined, stop?: number | undefined) => Pipeline<[...TCommands, Command<any, (number | null)[]>]>;
        /**
         * @see https://redis.io/commands/json.clear
         */
        clear: (key: string, path?: string | undefined) => Pipeline<[...TCommands, Command<any, number>]>;
        /**
         * @see https://redis.io/commands/json.del
         */
        del: (key: string, path?: string | undefined) => Pipeline<[...TCommands, Command<any, number>]>;
        /**
         * @see https://redis.io/commands/json.forget
         */
        forget: (key: string, path?: string | undefined) => Pipeline<[...TCommands, Command<any, number>]>;
        /**
         * @see https://redis.io/commands/geoadd
         */
        geoadd: (args_0: string, args_1: GeoAddCommandOptions | GeoMember<unknown>, ...args_2: GeoMember<unknown>[]) => Pipeline<[...TCommands, Command<any, number | null>]>;
        /**
         * @see https://redis.io/commands/geodist
         */
        geodist: (key: string, member1: unknown, member2: unknown, unit?: "M" | "KM" | "FT" | "MI" | undefined) => Pipeline<[...TCommands, Command<any, number | null>]>;
        /**
         * @see https://redis.io/commands/geopos
         */
        geopos: (args_0: string, ...args_1: unknown[]) => Pipeline<[...TCommands, Command<any, {
            lng: number;
            lat: number;
        }[]>]>;
        /**
         * @see https://redis.io/commands/geohash
         */
        geohash: (args_0: string, ...args_1: unknown[]) => Pipeline<[...TCommands, Command<any, (string | null)[]>]>;
        /**
         * @see https://redis.io/commands/geosearch
         */
        geosearch: (key: string, centerPoint: {
            type: "FROMLONLAT" | "fromlonlat";
            coordinate: {
                lon: number;
                lat: number;
            };
        } | {
            type: "FROMMEMBER" | "frommember";
            member: unknown;
        }, shape: {
            type: "BYRADIUS" | "byradius";
            radius: number;
            radiusType: "M" | "KM" | "FT" | "MI";
        } | {
            type: "BYBOX" | "bybox";
            rect: {
                width: number;
                height: number;
            };
            rectType: "M" | "KM" | "FT" | "MI";
        }, order: "ASC" | "DESC" | "asc" | "desc", opts?: {
            count?: {
                limit: number;
                any?: boolean | undefined;
            } | undefined;
            withCoord?: boolean | undefined;
            withDist?: boolean | undefined;
            withHash?: boolean | undefined;
        } | undefined) => Pipeline<[...TCommands, Command<any, ({
            member: unknown;
        } & {
            coord?: {
                long: number;
                lat: number;
            } | undefined;
            dist?: number | undefined;
            hash?: string | undefined;
        })[]>]>;
        /**
         * @see https://redis.io/commands/geosearchstore
         */
        geosearchstore: (destination: string, key: string, centerPoint: {
            type: "FROMLONLAT" | "fromlonlat";
            coordinate: {
                lon: number;
                lat: number;
            };
        } | {
            type: "FROMMEMBER" | "frommember";
            member: unknown;
        }, shape: {
            type: "BYRADIUS" | "byradius";
            radius: number;
            radiusType: "M" | "KM" | "FT" | "MI";
        } | {
            type: "BYBOX" | "bybox";
            rect: {
                width: number;
                height: number;
            };
            rectType: "M" | "KM" | "FT" | "MI";
        }, order: "ASC" | "DESC" | "asc" | "desc", opts?: {
            count?: {
                limit: number;
                any?: boolean | undefined;
            } | undefined;
            storeDist?: boolean | undefined;
        } | undefined) => Pipeline<[...TCommands, Command<any, number>]>;
        /**
         * @see https://redis.io/commands/json.get
         */
        get: (...args: CommandArgs<typeof JsonGetCommand>) => Pipeline<[...TCommands, Command<any, any>]>;
        /**
         * @see https://redis.io/commands/json.mget
         */
        mget: (keys: string[], path: string) => Pipeline<[...TCommands, Command<any, any>]>;
        /**
         * @see https://redis.io/commands/json.numincrby
         */
        numincrby: (key: string, path: string, value: number) => Pipeline<[...TCommands, Command<any, (number | null)[]>]>;
        /**
         * @see https://redis.io/commands/json.nummultby
         */
        nummultby: (key: string, path: string, value: number) => Pipeline<[...TCommands, Command<any, (number | null)[]>]>;
        /**
         * @see https://redis.io/commands/json.objkeys
         */
        objkeys: (key: string, path?: string | undefined) => Pipeline<[...TCommands, Command<any, (string[] | null)[]>]>;
        /**
         * @see https://redis.io/commands/json.objlen
         */
        objlen: (key: string, path?: string | undefined) => Pipeline<[...TCommands, Command<any, (number | null)[]>]>;
        /**
         * @see https://redis.io/commands/json.resp
         */
        resp: (key: string, path?: string | undefined) => Pipeline<[...TCommands, Command<any, any>]>;
        /**
         * @see https://redis.io/commands/json.set
         */
        set: (key: string, path: string, value: string | number | boolean | Record<string, unknown> | (string | number | boolean | Record<string, unknown>)[], opts?: {
            nx: true;
            xx?: undefined;
        } | {
            nx?: undefined;
            xx: true;
        } | undefined) => Pipeline<[...TCommands, Command<any, "OK" | null>]>;
        /**
         * @see https://redis.io/commands/json.strappend
         */
        strappend: (key: string, path: string, value: string) => Pipeline<[...TCommands, Command<any, (number | null)[]>]>;
        /**
         * @see https://redis.io/commands/json.strlen
         */
        strlen: (key: string, path?: string | undefined) => Pipeline<[...TCommands, Command<any, (number | null)[]>]>;
        /**
         * @see https://redis.io/commands/json.toggle
         */
        toggle: (key: string, path: string) => Pipeline<[...TCommands, Command<any, number[]>]>;
        /**
         * @see https://redis.io/commands/json.type
         */
        type: (key: string, path?: string | undefined) => Pipeline<[...TCommands, Command<any, string[]>]>;
    };
}

/**
 * Creates a new script.
 *
 * Scripts offer the ability to optimistically try to execute a script without having to send the
 * entire script to the server. If the script is loaded on the server, it tries again by sending
 * the entire script. Afterwards, the script is cached on the server.
 *
 * @example
 * ```ts
 * const redis = new Redis({...})
 *
 * const script = redis.createScript<string>("return ARGV[1];")
 * const arg1 = await script.eval([], ["Hello World"])
 * expect(arg1, "Hello World")
 * ```
 */
declare class Script<TResult = unknown> {
    readonly script: string;
    readonly sha1: string;
    private readonly redis;
    constructor(redis: Redis, script: string);
    /**
     * Send an `EVAL` command to redis.
     */
    eval(keys: string[], args: string[]): Promise<TResult>;
    /**
     * Calculates the sha1 hash of the script and then calls `EVALSHA`.
     */
    evalsha(keys: string[], args: string[]): Promise<TResult>;
    /**
     * Optimistically try to run `EVALSHA` first.
     * If the script is not loaded in redis, it will fall back and try again with `EVAL`.
     *
     * Following calls will be able to use the cached script
     */
    exec(keys: string[], args: string[]): Promise<TResult>;
    /**
     * Compute the sha1 hash of the script and return its hex representation.
     */
    private digest;
}

/**
 * Serverless redis client for upstash.
 */
declare class Redis {
    protected client: Requester;
    protected opts?: CommandOptions<any, any>;
    protected enableTelemetry: boolean;
    /**
     * Create a new redis client
     *
     * @example
     * ```typescript
     * const redis = new Redis({
     *  url: "<UPSTASH_REDIS_REST_URL>",
     *  token: "<UPSTASH_REDIS_REST_TOKEN>",
     * });
     * ```
     */
    constructor(client: Requester, opts?: RedisOptions);
    get json(): {
        /**
         * @see https://redis.io/commands/json.arrappend
         */
        arrappend: (key: string, path: string, ...values: unknown[]) => Promise<(number | null)[]>;
        /**
         * @see https://redis.io/commands/json.arrindex
         */
        arrindex: (key: string, path: string, value: unknown, start?: number | undefined, stop?: number | undefined) => Promise<(number | null)[]>;
        /**
         * @see https://redis.io/commands/json.arrinsert
         */
        arrinsert: (key: string, path: string, index: number, ...values: unknown[]) => Promise<(number | null)[]>;
        /**
         * @see https://redis.io/commands/json.arrlen
         */
        arrlen: (key: string, path?: string | undefined) => Promise<(number | null)[]>;
        /**
         * @see https://redis.io/commands/json.arrpop
         */
        arrpop: (key: string, path?: string | undefined, index?: number | undefined) => Promise<unknown[]>;
        /**
         * @see https://redis.io/commands/json.arrtrim
         */
        arrtrim: (key: string, path?: string | undefined, start?: number | undefined, stop?: number | undefined) => Promise<(number | null)[]>;
        /**
         * @see https://redis.io/commands/json.clear
         */
        clear: (key: string, path?: string | undefined) => Promise<number>;
        /**
         * @see https://redis.io/commands/json.del
         */
        del: (key: string, path?: string | undefined) => Promise<number>;
        /**
         * @see https://redis.io/commands/json.forget
         */
        forget: (key: string, path?: string | undefined) => Promise<number>;
        /**
         * @see https://redis.io/commands/geoadd
         */
        geoadd: (args_0: string, args_1: GeoAddCommandOptions | GeoMember<unknown>, ...args_2: GeoMember<unknown>[]) => Promise<number | null>;
        /**
         * @see https://redis.io/commands/geopos
         */
        geopos: (args_0: string, ...args_1: unknown[]) => Promise<{
            lng: number;
            lat: number;
        }[]>;
        /**
         * @see https://redis.io/commands/geodist
         */
        geodist: (key: string, member1: unknown, member2: unknown, unit?: "M" | "KM" | "FT" | "MI" | undefined) => Promise<number | null>;
        /**
         * @see https://redis.io/commands/geohash
         */
        geohash: (args_0: string, ...args_1: unknown[]) => Promise<(string | null)[]>;
        /**
         * @see https://redis.io/commands/geosearch
         */
        geosearch: (key: string, centerPoint: {
            type: "FROMLONLAT" | "fromlonlat";
            coordinate: {
                lon: number;
                lat: number;
            };
        } | {
            type: "FROMMEMBER" | "frommember";
            member: unknown;
        }, shape: {
            type: "BYRADIUS" | "byradius";
            radius: number;
            radiusType: "M" | "KM" | "FT" | "MI";
        } | {
            type: "BYBOX" | "bybox";
            rect: {
                width: number;
                height: number;
            };
            rectType: "M" | "KM" | "FT" | "MI";
        }, order: "ASC" | "DESC" | "asc" | "desc", opts?: {
            count?: {
                limit: number;
                any?: boolean | undefined;
            } | undefined;
            withCoord?: boolean | undefined;
            withDist?: boolean | undefined;
            withHash?: boolean | undefined;
        } | undefined) => Promise<({
            member: unknown;
        } & {
            coord?: {
                long: number;
                lat: number;
            } | undefined;
            dist?: number | undefined;
            hash?: string | undefined;
        })[]>;
        /**
         * @see https://redis.io/commands/geosearchstore
         */
        geosearchstore: (destination: string, key: string, centerPoint: {
            type: "FROMLONLAT" | "fromlonlat";
            coordinate: {
                lon: number;
                lat: number;
            };
        } | {
            type: "FROMMEMBER" | "frommember";
            member: unknown;
        }, shape: {
            type: "BYRADIUS" | "byradius";
            radius: number;
            radiusType: "M" | "KM" | "FT" | "MI";
        } | {
            type: "BYBOX" | "bybox";
            rect: {
                width: number;
                height: number;
            };
            rectType: "M" | "KM" | "FT" | "MI";
        }, order: "ASC" | "DESC" | "asc" | "desc", opts?: {
            count?: {
                limit: number;
                any?: boolean | undefined;
            } | undefined;
            storeDist?: boolean | undefined;
        } | undefined) => Promise<number>;
        /**
         * @see https://redis.io/commands/json.get
         */
        get: (...args: CommandArgs<typeof JsonGetCommand>) => Promise<any>;
        /**
         * @see https://redis.io/commands/json.mget
         */
        mget: (keys: string[], path: string) => Promise<any>;
        /**
         * @see https://redis.io/commands/json.numincrby
         */
        numincrby: (key: string, path: string, value: number) => Promise<(number | null)[]>;
        /**
         * @see https://redis.io/commands/json.nummultby
         */
        nummultby: (key: string, path: string, value: number) => Promise<(number | null)[]>;
        /**
         * @see https://redis.io/commands/json.objkeys
         */
        objkeys: (key: string, path?: string | undefined) => Promise<(string[] | null)[]>;
        /**
         * @see https://redis.io/commands/json.objlen
         */
        objlen: (key: string, path?: string | undefined) => Promise<(number | null)[]>;
        /**
         * @see https://redis.io/commands/json.resp
         */
        resp: (key: string, path?: string | undefined) => Promise<any>;
        /**
         * @see https://redis.io/commands/json.set
         */
        set: (key: string, path: string, value: string | number | boolean | Record<string, unknown> | (string | number | boolean | Record<string, unknown>)[], opts?: {
            nx: true;
            xx?: undefined;
        } | {
            nx?: undefined;
            xx: true;
        } | undefined) => Promise<"OK" | null>;
        /**
         * @see https://redis.io/commands/json.strappend
         */
        strappend: (key: string, path: string, value: string) => Promise<(number | null)[]>;
        /**
         * @see https://redis.io/commands/json.strlen
         */
        strlen: (key: string, path?: string | undefined) => Promise<(number | null)[]>;
        /**
         * @see https://redis.io/commands/json.toggle
         */
        toggle: (key: string, path: string) => Promise<number[]>;
        /**
         * @see https://redis.io/commands/json.type
         */
        type: (key: string, path?: string | undefined) => Promise<string[]>;
    };
    /**
     * Wrap a new middleware around the HTTP client.
     */
    use: <TResult = unknown>(middleware: (r: UpstashRequest, next: <TResult_1 = unknown>(req: UpstashRequest) => Promise<UpstashResponse<TResult_1>>) => Promise<UpstashResponse<TResult>>) => void;
    /**
     * Technically this is not private, we can hide it from intellisense by doing this
     */
    protected addTelemetry: (telemetry: Telemetry) => void;
    createScript(script: string): Script;
    /**
     * Create a new pipeline that allows you to send requests in bulk.
     *
     * @see {@link Pipeline}
     */
    pipeline: () => Pipeline<[]>;
    /**
     * Create a new transaction to allow executing multiple steps atomically.
     *
     * All the commands in a transaction are serialized and executed sequentially. A request sent by
     * another client will never be served in the middle of the execution of a Redis Transaction. This
     * guarantees that the commands are executed as a single isolated operation.
     *
     * @see {@link Pipeline}
     */
    multi: () => Pipeline<[]>;
    /**
     * @see https://redis.io/commands/append
     */
    append: (key: string, value: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/bitcount
     */
    bitcount: (key: string, start: number, end: number) => Promise<number>;
    /**
     * @see https://redis.io/commands/bitop
     */
    bitop: {
        (op: "and" | "or" | "xor", destinationKey: string, sourceKey: string, ...sourceKeys: string[]): Promise<number>;
        (op: "not", destinationKey: string, sourceKey: string): Promise<number>;
    };
    /**
     * @see https://redis.io/commands/bitpos
     */
    bitpos: (key: string, bit: 0 | 1, start?: number | undefined, end?: number | undefined) => Promise<number>;
    /**
     * @see https://redis.io/commands/copy
     */
    copy: (key: string, destinationKey: string, opts?: {
        replace: boolean;
    } | undefined) => Promise<"COPIED" | "NOT_COPIED">;
    /**
     * @see https://redis.io/commands/dbsize
     */
    dbsize: () => Promise<number>;
    /**
     * @see https://redis.io/commands/decr
     */
    decr: (key: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/decrby
     */
    decrby: (key: string, decrement: number) => Promise<number>;
    /**
     * @see https://redis.io/commands/del
     */
    del: (...args: CommandArgs<typeof DelCommand>) => Promise<number>;
    /**
     * @see https://redis.io/commands/echo
     */
    echo: (message: string) => Promise<string>;
    /**
     * @see https://redis.io/commands/eval
     */
    eval: <TArgs extends unknown[], TData = unknown>(script: string, keys: string[], args: TArgs) => Promise<TData>;
    /**
     * @see https://redis.io/commands/evalsha
     */
    evalsha: <TArgs extends unknown[], TData = unknown>(sha1: string, keys: string[], args: TArgs) => Promise<TData>;
    /**
     * @see https://redis.io/commands/exists
     */
    exists: (...args: CommandArgs<typeof ExistsCommand>) => Promise<number>;
    /**
     * @see https://redis.io/commands/expire
     */
    expire: (key: string, seconds: number) => Promise<0 | 1>;
    /**
     * @see https://redis.io/commands/expireat
     */
    expireat: (key: string, unix: number) => Promise<0 | 1>;
    /**
     * @see https://redis.io/commands/flushall
     */
    flushall: (args?: CommandArgs<typeof FlushAllCommand>) => Promise<"OK">;
    /**
     * @see https://redis.io/commands/flushdb
     */
    flushdb: (opts?: {
        async?: boolean | undefined;
    } | undefined) => Promise<"OK">;
    /**
     * @see https://redis.io/commands/get
     */
    get: <TData>(key: string) => Promise<TData | null>;
    /**
     * @see https://redis.io/commands/getbit
     */
    getbit: (key: string, offset: number) => Promise<0 | 1>;
    /**
     * @see https://redis.io/commands/getdel
     */
    getdel: <TData>(key: string) => Promise<TData | null>;
    /**
     * @see https://redis.io/commands/getrange
     */
    getrange: (key: string, start: number, end: number) => Promise<string>;
    /**
     * @see https://redis.io/commands/getset
     */
    getset: <TData>(key: string, value: TData) => Promise<TData | null>;
    /**
     * @see https://redis.io/commands/hdel
     */
    hdel: (key: string, ...fields: string[]) => Promise<0 | 1>;
    /**
     * @see https://redis.io/commands/hexists
     */
    hexists: (key: string, field: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/hget
     */
    hget: <TData>(key: string, field: string) => Promise<TData | null>;
    /**
     * @see https://redis.io/commands/hgetall
     */
    hgetall: <TData extends Record<string, unknown>>(key: string) => Promise<TData | null>;
    /**
     * @see https://redis.io/commands/hincrby
     */
    hincrby: (key: string, field: string, increment: number) => Promise<number>;
    /**
     * @see https://redis.io/commands/hincrbyfloat
     */
    hincrbyfloat: (key: string, field: string, increment: number) => Promise<number>;
    /**
     * @see https://redis.io/commands/hkeys
     */
    hkeys: (key: string) => Promise<string[]>;
    /**
     * @see https://redis.io/commands/hlen
     */
    hlen: (key: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/hmget
     */
    hmget: <TData extends Record<string, unknown>>(key: string, ...fields: string[]) => Promise<TData | null>;
    /**
     * @see https://redis.io/commands/hmset
     */
    hmset: <TData>(key: string, kv: {
        [field: string]: TData;
    }) => Promise<"OK">;
    /**
     * @see https://redis.io/commands/hrandfield
     */
    hrandfield: {
        (key: string): Promise<string>;
        (key: string, count: number): Promise<string[]>;
        <TData extends Record<string, unknown>>(key: string, count: number, withValues: boolean): Promise<Partial<TData>>;
    };
    /**
     * @see https://redis.io/commands/hscan
     */
    hscan: (key: string, cursor: number, cmdOpts?: ScanCommandOptions | undefined) => Promise<[number, (string | number)[]]>;
    /**
     * @see https://redis.io/commands/hset
     */
    hset: <TData>(key: string, kv: {
        [field: string]: TData;
    }) => Promise<number>;
    /**
     * @see https://redis.io/commands/hsetnx
     */
    hsetnx: <TData>(key: string, field: string, value: TData) => Promise<0 | 1>;
    /**
     * @see https://redis.io/commands/hstrlen
     */
    hstrlen: (key: string, field: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/hvals
     */
    hvals: (key: string) => Promise<any>;
    /**
     * @see https://redis.io/commands/incr
     */
    incr: (key: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/incrby
     */
    incrby: (key: string, value: number) => Promise<number>;
    /**
     * @see https://redis.io/commands/incrbyfloat
     */
    incrbyfloat: (key: string, value: number) => Promise<number>;
    /**
     * @see https://redis.io/commands/keys
     */
    keys: (pattern: string) => Promise<string[]>;
    /**
     * @see https://redis.io/commands/lindex
     */
    lindex: (key: string, index: number) => Promise<any>;
    /**
     * @see https://redis.io/commands/linsert
     */
    linsert: <TData>(key: string, direction: "before" | "after", pivot: TData, value: TData) => Promise<number>;
    /**
     * @see https://redis.io/commands/llen
     */
    llen: (key: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/lmove
     */
    lmove: <TData = string>(source: string, destination: string, whereFrom: "left" | "right", whereTo: "left" | "right") => Promise<TData>;
    /**
     * @see https://redis.io/commands/lpop
     */
    lpop: <TData>(key: string, count?: number | undefined) => Promise<TData | null>;
    /**
     * @see https://redis.io/commands/lpos
     */
    lpos: <TData = number>(key: string, element: unknown, opts?: {
        rank?: number | undefined;
        count?: number | undefined;
        maxLen?: number | undefined;
    } | undefined) => Promise<TData>;
    /**
     * @see https://redis.io/commands/lpush
     */
    lpush: <TData>(key: string, ...elements: TData[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/lpushx
     */
    lpushx: <TData>(key: string, ...elements: TData[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/lrange
     */
    lrange: <TResult = string>(key: string, start: number, end: number) => Promise<TResult[]>;
    /**
     * @see https://redis.io/commands/lrem
     */
    lrem: <TData>(key: string, count: number, value: TData) => Promise<number>;
    /**
     * @see https://redis.io/commands/lset
     */
    lset: <TData>(key: string, index: number, value: TData) => Promise<"OK">;
    /**
     * @see https://redis.io/commands/ltrim
     */
    ltrim: (key: string, start: number, end: number) => Promise<"OK">;
    /**
     * @see https://redis.io/commands/mget
     */
    mget: <TData extends unknown[]>(...args: CommandArgs<typeof MGetCommand>) => Promise<TData>;
    /**
     * @see https://redis.io/commands/mset
     */
    mset: <TData>(kv: {
        [key: string]: TData;
    }) => Promise<"OK">;
    /**
     * @see https://redis.io/commands/msetnx
     */
    msetnx: <TData>(kv: {
        [key: string]: TData;
    }) => Promise<number>;
    /**
     * @see https://redis.io/commands/persist
     */
    persist: (key: string) => Promise<0 | 1>;
    /**
     * @see https://redis.io/commands/pexpire
     */
    pexpire: (key: string, milliseconds: number) => Promise<0 | 1>;
    /**
     * @see https://redis.io/commands/pexpireat
     */
    pexpireat: (key: string, unix: number) => Promise<0 | 1>;
    /**
   * @see https://redis.io/commands/pfadd
   */
    pfadd: (args_0: string, ...args_1: unknown[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/pfcount
     */
    pfcount: (args_0: string, ...args_1: string[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/pfmerge
     */
    pfmerge: (destination_key: string, ...args_1: string[]) => Promise<"OK">;
    /**
     * @see https://redis.io/commands/ping
     */
    ping: (args?: CommandArgs<typeof PingCommand>) => Promise<string>;
    /**
     * @see https://redis.io/commands/psetex
     */
    psetex: <TData>(key: string, ttl: number, value: TData) => Promise<string>;
    /**
     * @see https://redis.io/commands/pttl
     */
    pttl: (key: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/publish
     */
    publish: (channel: string, message: unknown) => Promise<number>;
    /**
     * @see https://redis.io/commands/randomkey
     */
    randomkey: () => Promise<string | null>;
    /**
     * @see https://redis.io/commands/rename
     */
    rename: (source: string, destination: string) => Promise<"OK">;
    /**
     * @see https://redis.io/commands/renamenx
     */
    renamenx: (source: string, destination: string) => Promise<0 | 1>;
    /**
     * @see https://redis.io/commands/rpop
     */
    rpop: <TData = string>(key: string, count?: number | undefined) => Promise<TData | null>;
    /**
     * @see https://redis.io/commands/rpush
     */
    rpush: <TData>(key: string, ...elements: TData[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/rpushx
     */
    rpushx: <TData>(key: string, ...elements: TData[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/sadd
     */
    sadd: <TData>(key: string, ...members: TData[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/scan
     */
    scan: (cursor: number, opts?: ScanCommandOptions | undefined) => Promise<[number, string[]]>;
    /**
     * @see https://redis.io/commands/scard
     */
    scard: (key: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/script-exists
     */
    scriptExists: (...args: CommandArgs<typeof ScriptExistsCommand>) => Promise<number[]>;
    /**
     * @see https://redis.io/commands/script-flush
     */
    scriptFlush: (opts?: ScriptFlushCommandOptions | undefined) => Promise<"OK">;
    /**
     * @see https://redis.io/commands/script-load
     */
    scriptLoad: (script: string) => Promise<string>;
    /**
     * @see https://redis.io/commands/sdiff
     */
    sdiff: (key: string, ...keys: string[]) => Promise<unknown[]>;
    /**
     * @see https://redis.io/commands/sdiffstore
     */
    sdiffstore: (destination: string, ...keys: string[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/set
     */
    set: <TData>(key: string, value: TData, opts?: SetCommandOptions) => Promise<"OK" | TData | null>;
    /**
     * @see https://redis.io/commands/setbit
     */
    setbit: (key: string, offset: number, value: 0 | 1) => Promise<0 | 1>;
    /**
     * @see https://redis.io/commands/setex
     */
    setex: <TData>(key: string, ttl: number, value: TData) => Promise<"OK">;
    /**
     * @see https://redis.io/commands/setnx
     */
    setnx: <TData>(key: string, value: TData) => Promise<number>;
    /**
     * @see https://redis.io/commands/setrange
     */
    setrange: (key: string, offset: number, value: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/sinter
     */
    sinter: (key: string, ...keys: string[]) => Promise<string[]>;
    /**
     * @see https://redis.io/commands/sinterstore
     */
    sinterstore: (destination: string, key: string, ...keys: string[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/sismember
     */
    sismember: <TData>(key: string, member: TData) => Promise<0 | 1>;
    /**
     * @see https://redis.io/commands/smismember
     */
    smismember: <TMembers extends unknown[]>(key: string, members: TMembers) => Promise<(0 | 1)[]>;
    /**
     * @see https://redis.io/commands/smembers
     */
    smembers: <TData extends unknown[] = string[]>(key: string) => Promise<TData>;
    /**
     * @see https://redis.io/commands/smove
     */
    smove: <TData>(source: string, destination: string, member: TData) => Promise<0 | 1>;
    /**
     * @see https://redis.io/commands/spop
     */
    spop: <TData>(key: string, count?: number | undefined) => Promise<TData | null>;
    /**
     * @see https://redis.io/commands/srandmember
     */
    srandmember: <TData>(key: string, count?: number | undefined) => Promise<TData | null>;
    /**
     * @see https://redis.io/commands/srem
     */
    srem: <TData>(key: string, ...members: TData[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/sscan
     */
    sscan: (key: string, cursor: number, opts?: ScanCommandOptions | undefined) => Promise<[number, (string | number)[]]>;
    /**
     * @see https://redis.io/commands/strlen
     */
    strlen: (key: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/sunion
     */
    sunion: (key: string, ...keys: string[]) => Promise<unknown[]>;
    /**
     * @see https://redis.io/commands/sunionstore
     */
    sunionstore: (destination: string, key: string, ...keys: string[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/time
     */
    time: () => Promise<[number, number]>;
    /**
     * @see https://redis.io/commands/touch
     */
    touch: (...args: CommandArgs<typeof TouchCommand>) => Promise<number>;
    /**
     * @see https://redis.io/commands/ttl
     */
    ttl: (key: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/type
     */
    type: (key: string) => Promise<Type>;
    /**
     * @see https://redis.io/commands/unlink
     */
    unlink: (...args: CommandArgs<typeof UnlinkCommand>) => Promise<number>;
    /**
     * @see https://redis.io/commands/xadd
     */
    xadd: (key: string, id: string, entries: {
        [field: string]: unknown;
    }, opts?: {
        nomkStream?: boolean | undefined;
        trim?: (({
            type: "MAXLEN" | "maxlen";
            threshold: number;
        } | {
            type: "MINID" | "minid";
            threshold: string;
        }) & ({
            comparison: "~";
            limit?: number | undefined;
        } | {
            comparison: "=";
            limit?: undefined;
        })) | undefined;
    } | undefined) => Promise<string>;
    /**
     * @see https://redis.io/commands/xrange
     */
    xrange: (key: string, start: string, end: string, count?: number | undefined) => Promise<Record<string, Record<string, unknown>>>;
    /**
     * @see https://redis.io/commands/zadd
     */
    zadd: <TData>(...args: [key: string, scoreMember: ScoreMember<TData>, ...scoreMemberPairs: ScoreMember<TData>[]] | [key: string, opts: ZAddCommandOptions | ZAddCommandOptionsWithIncr, ScoreMember<TData>, ...ScoreMember<TData>[]]) => Promise<number | null>;
    /**
     * @see https://redis.io/commands/zcard
     */
    zcard: (key: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/zcount
     */
    zcount: (key: string, min: string | number, max: string | number) => Promise<number>;
    /**
     * @see https://redis.io/commands/zdiffstore
     */
    zdiffstore: (destination: string, numkeys: number, ...keys: string[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/zincrby
     */
    zincrby: <TData>(key: string, increment: number, member: TData) => Promise<number>;
    /**
     * @see https://redis.io/commands/zinterstore
     */
    zinterstore: (destination: string, numKeys: number, keys: string[], opts?: ZInterStoreCommandOptions | undefined) => Promise<number>;
    /**
     * @see https://redis.io/commands/zlexcount
     */
    zlexcount: (key: string, min: string, max: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/zmscore
     */
    zmscore: (key: string, members: unknown[]) => Promise<number[] | null>;
    /**
     * @see https://redis.io/commands/zpopmax
     */
    zpopmax: <TData>(key: string, count?: number | undefined) => Promise<TData[]>;
    /**
     * @see https://redis.io/commands/zpopmin
     */
    zpopmin: <TData>(key: string, count?: number | undefined) => Promise<TData[]>;
    /**
     * @see https://redis.io/commands/zrange
     */
    zrange: <TData extends unknown[]>(...args: [key: string, min: number, max: number, opts?: ZRangeCommandOptions] | [
        key: string,
        min: `(${string}` | `[${string}` | "-" | "+",
        max: `(${string}` | `[${string}` | "-" | "+",
        opts: {
            byLex: true;
        } & ZRangeCommandOptions
    ] | [
        key: string,
        min: number | `(${number}` | "-inf" | "+inf",
        max: number | `(${number}` | "-inf" | "+inf",
        opts: {
            byScore: true;
        } & ZRangeCommandOptions
    ]) => Promise<TData>;
    /**
     * @see https://redis.io/commands/zrank
     */
    zrank: <TData>(key: string, member: TData) => Promise<number | null>;
    /**
     * @see https://redis.io/commands/zrem
     */
    zrem: <TData>(key: string, ...members: TData[]) => Promise<number>;
    /**
     * @see https://redis.io/commands/zremrangebylex
     */
    zremrangebylex: (key: string, min: string, max: string) => Promise<number>;
    /**
     * @see https://redis.io/commands/zremrangebyrank
     */
    zremrangebyrank: (key: string, start: number, stop: number) => Promise<number>;
    /**
     * @see https://redis.io/commands/zremrangebyscore
     */
    zremrangebyscore: (key: string, min: number, max: number) => Promise<number>;
    /**
     * @see https://redis.io/commands/zrevrank
     */
    zrevrank: <TData>(key: string, member: TData) => Promise<number | null>;
    /**
     * @see https://redis.io/commands/zscan
     */
    zscan: (key: string, cursor: number, opts?: ScanCommandOptions | undefined) => Promise<[number, (string | number)[]]>;
    /**
     * @see https://redis.io/commands/zscore
     */
    zscore: <TData>(key: string, member: TData) => Promise<number | null>;
    /**
     * @see https://redis.io/commands/zunion
     */
    zunion: (numKeys: number, keys: string[], opts?: ZUnionCommandOptions | undefined) => Promise<any>;
    /**
     * @see https://redis.io/commands/zunionstore
     */
    zunionstore: (destination: string, numKeys: number, keys: string[], opts?: ZUnionStoreCommandOptions | undefined) => Promise<number>;
}

/**
 * @see https://redis.io/commands/zdiffstore
 */
declare class ZDiffStoreCommand extends Command<number, number> {
    constructor(cmd: [destination: string, numkeys: number, ...keys: string[]], opts?: CommandOptions<number, number>);
}

/**
 * @see https://redis.io/commands/zmscore
 */
declare class ZMScoreCommand<TData> extends Command<string[] | null, number[] | null> {
    constructor(cmd: [key: string, members: TData[]], opts?: CommandOptions<string[] | null, number[] | null>);
}

export { IncrByCommand as $, AppendCommand as A, BitCountCommand as B, CopyCommand as C, DBSizeCommand as D, EchoCommand as E, FlushAllCommand as F, GeoAddCommand as G, GetSetCommand as H, HDelCommand as I, HExistsCommand as J, HGetCommand as K, HGetAllCommand as L, HIncrByCommand as M, HIncrByFloatCommand as N, HKeysCommand as O, HLenCommand as P, HMGetCommand as Q, RedisOptions as R, HMSetCommand as S, HRandFieldCommand as T, UpstashRequest as U, HScanCommand as V, HSetCommand as W, HSetNXCommand as X, HStrLenCommand as Y, HValsCommand as Z, IncrCommand as _, RequesterConfig as a, SetNxCommand as a$, IncrByFloatCommand as a0, JsonArrAppendCommand as a1, JsonArrIndexCommand as a2, JsonArrInsertCommand as a3, JsonArrLenCommand as a4, JsonArrPopCommand as a5, JsonArrTrimCommand as a6, JsonClearCommand as a7, JsonDelCommand as a8, JsonForgetCommand as a9, MSetNXCommand as aA, PersistCommand as aB, PExpireCommand as aC, PExpireAtCommand as aD, PingCommand as aE, PSetEXCommand as aF, PTtlCommand as aG, PublishCommand as aH, RandomKeyCommand as aI, RenameCommand as aJ, RenameNXCommand as aK, RPopCommand as aL, RPushCommand as aM, RPushXCommand as aN, SAddCommand as aO, ScanCommand as aP, ScanCommandOptions as aQ, SCardCommand as aR, ScriptExistsCommand as aS, ScriptFlushCommand as aT, ScriptLoadCommand as aU, SDiffCommand as aV, SDiffStoreCommand as aW, SetCommand as aX, SetCommandOptions as aY, SetBitCommand as aZ, SetExCommand as a_, JsonGetCommand as aa, JsonMGetCommand as ab, JsonNumIncrByCommand as ac, JsonNumMultByCommand as ad, JsonObjKeysCommand as ae, JsonObjLenCommand as af, JsonRespCommand as ag, JsonSetCommand as ah, JsonStrAppendCommand as ai, JsonStrLenCommand as aj, JsonToggleCommand as ak, JsonTypeCommand as al, KeysCommand as am, LIndexCommand as an, LInsertCommand as ao, LLenCommand as ap, LMoveCommand as aq, LPopCommand as ar, LPushCommand as as, LPushXCommand as at, LRangeCommand as au, LRemCommand as av, LSetCommand as aw, LTrimCommand as ax, MGetCommand as ay, MSetCommand as az, Redis as b, SetRangeCommand as b0, SInterCommand as b1, SInterStoreCommand as b2, SIsMemberCommand as b3, SMembersCommand as b4, SMIsMemberCommand as b5, SMoveCommand as b6, SPopCommand as b7, SRandMemberCommand as b8, SRemCommand as b9, ZRangeCommand as bA, ZRangeCommandOptions as bB, ZRankCommand as bC, ZRemCommand as bD, ZRemRangeByLexCommand as bE, ZRemRangeByRankCommand as bF, ZRemRangeByScoreCommand as bG, ZRevRankCommand as bH, ZScanCommand as bI, ZScoreCommand as bJ, ZUnionCommand as bK, ZUnionCommandOptions as bL, ZUnionStoreCommand as bM, ZUnionStoreCommandOptions as bN, SScanCommand as ba, StrLenCommand as bb, SUnionCommand as bc, SUnionStoreCommand as bd, TimeCommand as be, TouchCommand as bf, TtlCommand as bg, Type as bh, TypeCommand as bi, UnlinkCommand as bj, XAddCommand as bk, XRangeCommand as bl, ScoreMember as bm, ZAddCommandOptions as bn, ZAddCommandOptionsWithIncr as bo, ZAddCommand as bp, ZCardCommand as bq, ZCountCommand as br, ZDiffStoreCommand as bs, ZIncrByCommand as bt, ZInterStoreCommand as bu, ZInterStoreCommandOptions as bv, ZLexCountCommand as bw, ZMScoreCommand as bx, ZPopMaxCommand as by, ZPopMinCommand as bz, Requester as c, UpstashResponse as d, BitOpCommand as e, BitPosCommand as f, DecrCommand as g, DecrByCommand as h, DelCommand as i, EvalCommand as j, EvalshaCommand as k, ExistsCommand as l, ExpireCommand as m, ExpireAtCommand as n, FlushDBCommand as o, GeoAddCommandOptions as p, GeoMember as q, GeoDistCommand as r, GeoHashCommand as s, GeoPosCommand as t, GeoSearchCommand as u, GeoSearchStoreCommand as v, GetCommand as w, GetBitCommand as x, GetDelCommand as y, GetRangeCommand as z };
