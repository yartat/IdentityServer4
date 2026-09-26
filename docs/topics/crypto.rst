.. _refCrypto:
Cryptography, Keys and HTTPS
============================

IdentityServer relies on a couple of crypto mechanisms to do its job.

Token signing and validation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^
IdentityServer needs an asymmetric key pair to sign and validate JWTs. 
This keymaterial can be either packaged as a certificate or just raw keys.
Both RSA and ECDSA keys are supported and the supported signing algorithms are: RS256, RS384, RS512, PS256, PS384, PS512, ES256, ES384 and ES512.

You can use multiple signing keys simultaneously, but only one signing key per algorithm is supported.
The first signing key you register is considered the default signing key.

Both :ref:`clients <refClient>` and :ref:`API resources <refApiResource>` can express preferences on the signing algorithm.
If you request a single token for multiple API resources, all resources need to agree on at least one allowed signing algorithm.

Loading of signing key and the corresponding validation part is done by implementations of ``ISigningCredentialStore`` and ``IValidationKeysStore``.
If you want to customize the loading of the keys, you can implement those interfaces and register them with DI.

The DI builder extensions has a couple of convenience methods to set signing and validation keys - see :ref:`here <refStartupKeyMaterial>`.

Signing key rollover
^^^^^^^^^^^^^^^^^^^^
While you can only use one signing key at a time, you can publish more than one validation key to the discovery document.
This is useful for key rollover.

In a nutshell, a rollover typically works like this:

1. you request/create new key material
2. you publish the new validation key in addition to the current one. You can use the ``AddValidationKey`` builder extension method for that.
3. all clients and APIs now have a chance to learn about the new key the next time they update their local copy of the discovery document
4. after a certain amount of time (e.g. 24h) all clients and APIs should now accept both the old and the new key material
5. keep the old key material around for as long as you like, maybe you have long-lived tokens that need validation
6. retire the old key material when it is not used anymore
7. all clients and APIs will "forget" the old key next time they update their local copy of the discovery document

This requires that clients and APIs use the discovery document, and also have a feature to periodically refresh their configuration.

Brock wrote a more detailed `blog post <https://brockallen.com/2019/08/09/identityserver-and-signing-key-rotation/>`_ about key rotation.
Automatic key management (see below) takes care of all those details.

Automatic key management
^^^^^^^^^^^^^^^^^^^^^^^^
IdentityServer can create, rotate and retire signing keys itself. It is disabled by default::

    services.AddIdentityServer(options =>
    {
        options.KeyManagement.Enabled = true;
    });

For every algorithm in ``KeyManagement.SigningAlgorithms`` (default ``RS256``) a key goes through these stages:

* a new key is published in the discovery document for ``PropagationTime`` (default 14 days) before it is used for signing
* it signs tokens until it is ``RotationInterval`` old (default 90 days); its successor is created ``PropagationTime`` before that
* it is published for validation for another ``RetentionDuration`` (default 14 days)
* it is then retired and deleted from the store (``DeleteRetiredKeys``)

When no usable key exists (first start, or the server was down past a rotation) a new key is used right away.
Keys added with ``AddSigningCredential`` take precedence for signing; both kinds of keys are published.

Keys are protected with ASP.NET Core data protection (``DataProtectKeys``), so data protection has to be configured
with keys shared by all instances of a server farm. They are stored in the ``KeyPath`` directory by default;
``AddOperationalStore`` of the EntityFramework integration stores them in the ``Keys`` table instead
(add a migration for ``PersistedGrantDbContext``). Custom stores implement ``ISigningKeyStore`` and are registered with ``AddSigningKeyStore<T>()``.

Loaded keys are cached for ``KeyCacheDuration`` (default 24 hours), which has to be shorter than ``PropagationTime``,
so keys created by other instances are published before they are used.

Data protection
^^^^^^^^^^^^^^^
Cookie authentication in ASP.NET Core (or anti-forgery in MVC) use the ASP.NET Core data protection feature.
Depending on your deployment scenario, this might require additional configuration. See the Microsoft `docs <https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview>`_ for more information.

HTTPS
^^^^^
We don't enforce the use of HTTPS, but for production it is mandatory for every interaction with IdentityServer.
