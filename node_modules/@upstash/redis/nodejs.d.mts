import { R as RedisOptions, a as RequesterConfig, b as Redis$1, c as Requester } from './zmscore-b6b93f14.js';
export { A as AppendCommand, B as BitCountCommand, e as BitOpCommand, f as BitPosCommand, C as CopyCommand, D as DBSizeCommand, h as DecrByCommand, g as DecrCommand, i as DelCommand, E as EchoCommand, j as EvalCommand, k as EvalshaCommand, l as ExistsCommand, n as ExpireAtCommand, m as ExpireCommand, F as FlushAllCommand, o as FlushDBCommand, G as GeoAddCommand, p as GeoAddCommandOptions, r as GeoDistCommand, s as GeoHashCommand, q as GeoMember, t as GeoPosCommand, u as GeoSearchCommand, v as GeoSearchStoreCommand, x as GetBitCommand, w as GetCommand, y as GetDelCommand, z as GetRangeCommand, H as GetSetCommand, I as HDelCommand, J as HExistsCommand, L as HGetAllCommand, K as HGetCommand, M as HIncrByCommand, N as HIncrByFloatCommand, O as HKeysCommand, P as HLenCommand, Q as HMGetCommand, S as HMSetCommand, T as HRandFieldCommand, V as HScanCommand, W as HSetCommand, X as HSetNXCommand, Y as HStrLenCommand, Z as HValsCommand, $ as IncrByCommand, a0 as IncrByFloatCommand, _ as IncrCommand, a1 as JsonArrAppendCommand, a2 as JsonArrIndexCommand, a3 as JsonArrInsertCommand, a4 as JsonArrLenCommand, a5 as JsonArrPopCommand, a6 as JsonArrTrimCommand, a7 as JsonClearCommand, a8 as JsonDelCommand, a9 as JsonForgetCommand, aa as JsonGetCommand, ab as JsonMGetCommand, ac as JsonNumIncrByCommand, ad as JsonNumMultByCommand, ae as JsonObjKeysCommand, af as JsonObjLenCommand, ag as JsonRespCommand, ah as JsonSetCommand, ai as JsonStrAppendCommand, aj as JsonStrLenCommand, ak as JsonToggleCommand, al as JsonTypeCommand, am as KeysCommand, an as LIndexCommand, ao as LInsertCommand, ap as LLenCommand, aq as LMoveCommand, ar as LPopCommand, as as LPushCommand, at as LPushXCommand, au as LRangeCommand, av as LRemCommand, aw as LSetCommand, ax as LTrimCommand, ay as MGetCommand, az as MSetCommand, aA as MSetNXCommand, aD as PExpireAtCommand, aC as PExpireCommand, aF as PSetEXCommand, aG as PTtlCommand, aB as PersistCommand, aE as PingCommand, aH as PublishCommand, aL as RPopCommand, aM as RPushCommand, aN as RPushXCommand, aI as RandomKeyCommand, aJ as RenameCommand, aK as RenameNXCommand, aO as SAddCommand, aR as SCardCommand, aV as SDiffCommand, aW as SDiffStoreCommand, b1 as SInterCommand, b2 as SInterStoreCommand, b3 as SIsMemberCommand, b5 as SMIsMemberCommand, b4 as SMembersCommand, b6 as SMoveCommand, b7 as SPopCommand, b8 as SRandMemberCommand, b9 as SRemCommand, ba as SScanCommand, bc as SUnionCommand, bd as SUnionStoreCommand, aP as ScanCommand, aQ as ScanCommandOptions, bm as ScoreMember, aS as ScriptExistsCommand, aT as ScriptFlushCommand, aU as ScriptLoadCommand, aZ as SetBitCommand, aX as SetCommand, aY as SetCommandOptions, a_ as SetExCommand, a$ as SetNxCommand, b0 as SetRangeCommand, bb as StrLenCommand, be as TimeCommand, bf as TouchCommand, bg as TtlCommand, bh as Type, bi as TypeCommand, bj as UnlinkCommand, U as UpstashRequest, d as UpstashResponse, bk as XAddCommand, bl as XRangeCommand, bp as ZAddCommand, bn as ZAddCommandOptions, bo as ZAddCommandOptionsWithIncr, bq as ZCardCommand, br as ZCountCommand, bs as ZDiffStoreCommand, bt as ZIncrByCommand, bu as ZInterStoreCommand, bv as ZInterStoreCommandOptions, bw as ZLexCountCommand, bx as ZMScoreCommand, by as ZPopMaxCommand, bz as ZPopMinCommand, bA as ZRangeCommand, bB as ZRangeCommandOptions, bC as ZRankCommand, bD as ZRemCommand, bE as ZRemRangeByLexCommand, bF as ZRemRangeByRankCommand, bG as ZRemRangeByScoreCommand, bH as ZRevRankCommand, bI as ZScanCommand, bJ as ZScoreCommand, bK as ZUnionCommand, bL as ZUnionCommandOptions, bM as ZUnionStoreCommand, bN as ZUnionStoreCommandOptions } from './zmscore-b6b93f14.js';

/**
 * Connection credentials for upstash redis.
 * Get them from https://console.upstash.com/redis/<uuid>
 */
type RedisConfigNodejs = {
    /**
     * UPSTASH_REDIS_REST_URL
     */
    url: string;
    /**
     * UPSTASH_REDIS_REST_TOKEN
     */
    token: string;
    /**
     * An agent allows you to reuse connections to reduce latency for multiple sequential requests.
     *
     * This is a node specific implementation and is not supported in various runtimes like Vercel
     * edge functions.
     *
     * @example
     * ```ts
     * import https from "https"
     *
     * const options: RedisConfigNodejs = {
     *  agent: new https.Agent({ keepAlive: true })
     * }
     * ```
     */
    agent?: any;
} & RedisOptions & RequesterConfig;
/**
 * Serverless redis client for upstash.
 */
declare class Redis extends Redis$1 {
    /**
     * Create a new redis client by providing the url and token
     *
     * @example
     * ```typescript
     * const redis = new Redis({
     *  url: "<UPSTASH_REDIS_REST_URL>",
     *  token: "<UPSTASH_REDIS_REST_TOKEN>",
     * });
     * ```
     */
    constructor(config: RedisConfigNodejs);
    /**
     * Create a new redis client by providing a custom `Requester` implementation
     *
     * @example
     * ```ts
     *
     * import { UpstashRequest, Requester, UpstashResponse, Redis } from "@upstash/redis"
     *
     *  const requester: Requester = {
     *    request: <TResult>(req: UpstashRequest): Promise<UpstashResponse<TResult>> => {
     *      // ...
     *    }
     *  }
     *
     * const redis = new Redis(requester)
     * ```
     */
    constructor(requesters: Requester);
    /**
     * Create a new Upstash Redis instance from environment variables.
     *
     * Use this to automatically load connection secrets from your environment
     * variables. For instance when using the Vercel integration.
     *
     * This tries to load `UPSTASH_REDIS_REST_URL` and `UPSTASH_REDIS_REST_TOKEN` from
     * your environment using `process.env`.
     */
    static fromEnv(config?: Omit<RedisConfigNodejs, "url" | "token">): Redis;
}

export { Redis, RedisConfigNodejs, Requester };
